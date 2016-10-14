﻿namespace Quickbird.Data
{
    using System;
    using System.Threading.Tasks;
    using Windows.UI.Xaml;

    internal class Sync
    {
        /// <summary>The Url of the web api that is used to fetch data.</summary>
        public const string ApiUrl = "https://ghapi46azure.azurewebsites.net/api";


        /// <summary>An complete task that can be have ContinueWith() called on it. Used to queue database
        /// tasks to make sure one completes before another starts.</summary>
        private Task _lastTask = Task.CompletedTask;

        private Sync() { }

        /// <summary>Singleton instance accessor.</summary>
        public static Sync Instance { get; } = new Sync();


        public async Task Update()
        {
            // Will do what SyncWithServerAsync does.
            throw new NotImplementedException();
        }

        /// <summary>The method should be executed on the UI thread, which means it should be called before any
        /// awaits, before the the method returns.</summary>
        private Task<T> AttachContinuationsAndSwapLastTask<T>(Func<T> workForNextTask)
        {
            var contTask = _lastTask.ContinueWith(_ => workForNextTask());
            _lastTask = contTask;
            ((App) Application.Current).AddSessionTask(contTask);
            return contTask;
        }
    }
}
