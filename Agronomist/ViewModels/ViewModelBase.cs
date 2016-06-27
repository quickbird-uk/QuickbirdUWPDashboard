namespace Agronomist.ViewModels
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using Windows.UI.Xaml;
    using JetBrains.Annotations;
    using Util;

    public class ViewModelBase : INotifyPropertyChanged
    {
        protected List<DispatcherTimer> DispatcherTimers = new List<DispatcherTimer>();

        public ViewModelBase()
        {
            Messenger.Instance.Suspending.Subscribe(OnSuspend);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnSuspend(bool isSuspendingIsntResuming)
        {
            if (isSuspendingIsntResuming)
            {
                SuspendDispacherTimers();
            }
            else
            {
                ResumeDispatcherTimers();
            }
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void SuspendDispacherTimers()
        {
            Debug.WriteLine("suspending timers");
            foreach (var dispatcherTimer in DispatcherTimers)
            {
                dispatcherTimer.Stop();
            }
        }

        private void ResumeDispatcherTimers()
        {
            Debug.WriteLine("resumuing timers");
            foreach (var dispatcherTimer in DispatcherTimers)
            {
                dispatcherTimer.Start();
            }
        }
    }
}