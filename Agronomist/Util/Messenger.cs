namespace Agronomist.Util
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Windows.ApplicationModel.Core;
    using Windows.UI.Core;

    /// <summary>
    ///     A middleman class for passing information between parts of the program.
    /// </summary>
    public class Messenger
    {
        public BroadcastMessage<string> NewDeviceDetected { get; } = new BroadcastMessage<string>();

        public BroadcastMessage<IEnumerable<SensorReading>> NewSensorDataPoint { get; } =
            new BroadcastMessage<IEnumerable<SensorReading>>();

        public BroadcastMessage<IEnumerable<RelayReading>> NewRelayDataPoint { get; } =
            new BroadcastMessage<IEnumerable<RelayReading>>();

        public BroadcastMessage<string> TablesChanged { get; } =
            new BroadcastMessage<string>();

        /// <summary>
        ///     True for suspending false for resuming.
        /// </summary>
        public BroadcastMessage<CompletionsSource> Suspending { get; } = new BroadcastMessage<bool>();

        public struct SensorReading
        {
            public SensorReading(Guid id, double value, DateTimeOffset timestamp, TimeSpan duration)
            {
                Value = value;
                Duration = duration;
                Timestamp = timestamp;
                SensorId = id;
            }

            public double Value { get; }

            public TimeSpan Duration { get; }

            public DateTimeOffset Timestamp { get; }

            public Guid SensorId { get; }
        }

        public struct RelayReading
        {
            public RelayReading(Guid id, bool state, DateTimeOffset timestamp, TimeSpan duration)
            {
                State = state;
                Duration = duration;
                Timestamp = timestamp;
                RelayId = id;
            }

            public bool State { get; }

            public TimeSpan Duration { get; }

            public DateTimeOffset Timestamp { get; }

            public Guid RelayId { get; }
        }

        /// <summary>
        ///     Broacast a message to multiple subscribers.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public class BroadcastMessage<T>
        {
            private readonly List<WeakReference<Action<T>>> _subscribers = new List<WeakReference<Action<T>>>();

            public void Subscribe(Action<T> action)
            {
                _subscribers.Add(new WeakReference<Action<T>>(action));
            }

            public async Task Invoke(T param, bool useCoreDispatcher = true)
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
                if (useCoreDispatcher) dispatcher = CoreApplication.MainView.CoreWindow.Dispatcher;

                // Looping through external lists complete, safe to await now.
                foreach (var action in actions)
                {
                    if (useCoreDispatcher)
                    {
                        await dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => action(param));
                    }
                    else
                    {
                        await Task.Run(() => action(param)).ConfigureAwait(false);
                    }
                }
            }
        }

        #region SingletonInit

        private Messenger()
        {
        }

        public static Messenger Instance { get; } = new Messenger();

        #endregion
    }
}