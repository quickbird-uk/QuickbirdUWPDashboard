namespace Quickbird.Util
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Windows.UI.Core;
    using Windows.UI.Xaml;

    /// <summary>Broacast a message to multiple subscribers.</summary>
    /// <typeparam name="T"></typeparam>
    public class BroadcastMessage<T>
    {
        private readonly List<WeakReference<Action<T>>> _subscribers = new List<WeakReference<Action<T>>>();

        /// <summary>Invokes all the subscribed actions that have not been garbage collected. References to
        /// non-existing objects are cleaned up.</summary>
        /// <param name="param">Parameter passed to actions</param>
        /// <param name="useCoreDispatcher">Default true, invoke on UI. When false uses Task.Run().</param>
        /// <param name="insertCompletionSource">If the param is a completion source this will create a new
        /// completion source for every action, await them all and then SetResult(null) on the original param
        /// completion source.</param>
        /// <returns>Awaitable.</returns>
        public async Task Invoke(T param, bool useCoreDispatcher = true, bool insertCompletionSource = false)
        {
            //The first half of this code has externally mutable lists, so no awaits.
            var actions = new List<Action<T>>();
            var deadActions = new List<WeakReference<Action<T>>>();

            foreach (var weakReference in _subscribers)
            {
                Action<T> target;
                if (weakReference.TryGetTarget(out target))
                    actions.Add(target);
                else
                    deadActions.Add(weakReference);
            }

            // Prune disposed actions (corresponding objects no longer exist).
            _subscribers.RemoveAll(deadActions.Contains);

            CoreDispatcher dispatcher = null;
            if (useCoreDispatcher) dispatcher = ((App) Application.Current).Dispatcher;
            if (dispatcher == null)
                Log.ShouldNeverHappen($"Messenger.Instance.Dispatcher null at BroadcastMessage.Invoke() {typeof(T)}");

            // Special mode when the param is a TaskCompletionSource<object> and you want it to be set when all the actions complete.
            if (insertCompletionSource)
            {
                var completers = new List<TaskCompletionSource<object>>();
                var originalCompleter = param as TaskCompletionSource<object>;

                if (null == originalCompleter) throw new NullReferenceException("This should be a completer.");

                foreach (var action in actions)
                {
                    var completingAction = action as Action<TaskCompletionSource<object>>;
                    if (completingAction == null) continue;

                    var completer = new TaskCompletionSource<object>();
                    completers.Add(completer);

                    if (useCoreDispatcher)
                    {
                        if (dispatcher != null)
                            await dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => completingAction(completer));
                    }
                    else
                    {
                        await Task.Run(() => completingAction(completer)).ConfigureAwait(false);
                    }
                }

                await Task.WhenAll(completers.Select(s => s.Task));
                originalCompleter.SetResult(null);
            }
            else
            {
                // Looping through external lists complete, safe to await now.
                foreach (var action in actions)
                {
                    if (useCoreDispatcher)
                    {
                        if (dispatcher != null)
                            await dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => action(param));
                    }
                    else
                    {
                        await Task.Run(() => action(param)).ConfigureAwait(false);
                    }
                }
            }
        }

        /// <summary>Puts an weakrefernce to an action on the list of actions to be used when the Invoke method
        /// is called.</summary>
        /// <param name="action">Action to be invoked on the Invoke method.</param>
        public void Subscribe(Action<T> action) { _subscribers.Add(new WeakReference<Action<T>>(action)); }

        /// <summary>Removes any weakreferences to the specified action immediately.</summary>
        /// <param name="action">Action to be removed.</param>
        public void Unsubscribe(Action<T> action)
        {
            _subscribers.RemoveAll(weakRef =>
            {
                Action<T> target;
                if (weakRef.TryGetTarget(out target))
                {
                    return target == action;
                }
                return false;
            });
        }
    }
}
