namespace Quickbird.Util
{
    using System;
    using System.Threading.Tasks;
    using Windows.UI.Core;
    using Windows.UI.Xaml;

    /// <summary>Use for dispatching code to the UI while blocking the current thread (unless it is the ui,
    /// then run normally).</summary>
    internal static class BlockingDispatcher
    {
        /// <summary>Synchronously runs code on the UI thread, taking into account if you are already on the UI
        /// thread.</summary>
        /// <param name="work">Work to be done on the UI thread.</param>
        public static void Run(Action work)
        {
            var dispatcher = ((App) Application.Current).Dispatcher;

            if (dispatcher.HasThreadAccess)
            {
                // This is the UI thread so just run the code normally.
                work();
            }
            else
            {
                // Must use a completer because RunAsync returns immediately, putting the task on the end of a queue.
                var completer = new TaskCompletionSource<object>();

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                ((App) Application.Current).Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    work();
                    completer.SetResult(null);
                });
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

                //Wait until the UI thread gets around to doing the work we gave it and finishes it.
                completer.Task.Wait();
            }
        }
    }
}
