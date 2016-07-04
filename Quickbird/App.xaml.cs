namespace Quickbird
{
    using System;
    using System.Threading.Tasks;
    using Windows.ApplicationModel;
    using Windows.ApplicationModel.Activation;
    using Windows.ApplicationModel.ExtendedExecution;
    using Windows.UI.Core;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;
    using Windows.UI.Xaml.Navigation;
    using LocalNetworking;
    using Microsoft.EntityFrameworkCore;
    using Models;
    using Util;
    using Views;
    using Internet;

    /// <summary>
    ///     Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : Application
    {
        private ExtendedExecutionSession _extendedExecutionSession;
        private Action<object> _manageLocalNetworkChangeEvent;

        private Manager _networking;
        private bool _notPrelaunchSuspend;
        private Frame _rootFrame;

        /// <summary>
        ///     Initializes the singleton application object.  This is the first line of authored code
        ///     executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            InitializeComponent();
            Suspending += OnSuspending;
            Resuming += OnResuming;
        }

        private async void OnResuming(object sender, object e)
        {
            Toast.Debug("OnResuming", "");
            var completer = new TaskCompletionSource<object>();
            await Messenger.Instance.Resuming.Invoke(completer, true, true);
            await completer.Task;
        }

        /// <summary>
        ///     Fired when the user attempts to open the program, even whent he program is already open.
        ///     This gets fired when the user clicks notifications, resulting in this being called in an already running
        ///     application.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected override async void OnLaunched(LaunchActivatedEventArgs e)
        {
            // Only initialise if the program is not already open.
            if (_rootFrame == null)
            {
                // Creates a frame and shoves it in the provided default window.
                _rootFrame = Window.Current.Content as Frame;
                _rootFrame = new Frame();
                _rootFrame.NavigationFailed += OnNavigationFailed;
                Window.Current.Content = _rootFrame;
                Window.Current.VisibilityChanged += OnVisibilityChanged;
            }

            // If the user launches the app when it is already open, just bring it to foreground.
            Window.Current.Activate();

            if (e.PrelaunchActivated)
            {
                // We must not try starting an extended session in this situation.
                Toast.Debug("OnLaunched", "Prelaunched");
            }
            else
            {
                // There could be a session open if the app is launched twice.
                if (_extendedExecutionSession == null)
                    await StartExtendedSession();
            }
        }

        private async void OnVisibilityChanged(object sender, VisibilityChangedEventArgs e)
        {
            Toast.Debug("OnVisibilityChanged", "");

            // _rootFrame will only ever be null here once.
            // If there is no content the app was prelaunched and we must navigate and begin the session.
            if (_rootFrame.Content == null)
            {
                using (var db = new MainDbContext())
                {
                    db.Database.Migrate();
                }

                _manageLocalNetworkChangeEvent = StartLocalNetworkManagerIfSettingsAllow;
                Messenger.Instance.DeviceManagementEnableChanged.Subscribe(_manageLocalNetworkChangeEvent);
                StartLocalNetworkManagerIfSettingsAllow();

                // Page navigation is somthing we do immediately after prelaunch is over.
                // Any suspend before this point could be assumed to be a part of prelaunch and be ignored.
                _notPrelaunchSuspend = true;

                _rootFrame.Navigate(Settings.Instance.CredsSet ? typeof(Shell) : typeof(LandingPage));

                Window.Current.Activate();

                if (_extendedExecutionSession == null)
                    await StartExtendedSession();
            }
        }

        /// <summary>
        ///     Starts or kills the local device network if the settings permit it.
        /// </summary>
        /// <param name="throwAway">An atefact of the BroadcastMessenger class, not used.</param>
        private void StartLocalNetworkManagerIfSettingsAllow(object throwAway = null)
        {
            if (Settings.Instance.LocalDeviceManagementEnabled)
            {
                // If it has already started, leave it alone.
                if (_networking == null)
                    _networking = new Manager();
            }
            else
            {
                // Shut it down if it has started.
                if (_networking != null)
                {
                    _networking.Dispose();
                    _networking = null;
                }
            }
        }

        /// <summary>
        ///     Invoked when Navigation to a certain page fails
        /// </summary>
        /// <param name="sender">The Frame which failed navigation</param>
        /// <param name="e">Details about the navigation failure</param>
        private void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        /// <summary>
        ///     Starts a new session, assuming the old one has been closed. I hope you did a null check.
        /// </summary>
        /// <returns>awaitable</returns>
        private async Task StartExtendedSession()
        {
            _extendedExecutionSession = new ExtendedExecutionSession
            {
                Reason = ExtendedExecutionReason.Unspecified,
                Description = "Live data needs to be synchronised to allow important alerts."
            };

            _extendedExecutionSession.Revoked += ExtendedExecutionRevoked;

            var result = await _extendedExecutionSession.RequestExtensionAsync();

            if (result == ExtendedExecutionResult.Allowed)
            {
                Toast.Debug("EES", "Success");
            }
            else
            {
                // The request has failed, kill will null it, and then the app will try again when the visibility changes.
                KillExtendedExecutionSession();
                Toast.NotifyUserOfError("Windows error, program may fail to record, sync and alert when minimised.");
            }
        }

        /// <summary>
        ///     Cleanly destroy an old session so that a new one can be requested.
        /// </summary>
        private void KillExtendedExecutionSession()
        {
            if (_extendedExecutionSession != null)
            {
                _extendedExecutionSession.Revoked -= ExtendedExecutionRevoked;
                _extendedExecutionSession.Dispose();
                _extendedExecutionSession = null;
            }
        }

        /// <summary>
        ///     Fired when Windows decides to kill a session.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private async void ExtendedExecutionRevoked(object sender, ExtendedExecutionRevokedEventArgs args)
        {
            KillExtendedExecutionSession(); // Null it.
            if (args.Reason == ExtendedExecutionRevokedReason.Resumed)
            {
                if (_extendedExecutionSession == null)
                    await StartExtendedSession();
            }
            else if (args.Reason == ExtendedExecutionRevokedReason.SystemPolicy)
            {
                // SystemPolicy could mean:
                // a. The app was closed by the user.
                // b. The app was terminated for system resources.
                // c. Something undocumented.
            }
        }

        /// <summary>
        ///     Lifecycle suspend.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        public async void OnSuspending(object sender, SuspendingEventArgs e)
        {
            Toast.Debug("OnSuspending", e.SuspendingOperation.ToString());

            // This is the most accurate thing that we can tell the user.
            // There is no way to know if the app is being terminated or just suspended for fun.
            if (_notPrelaunchSuspend)
                Toast.NotifyUserOfInformation("App is suspending.");
            else
                Toast.Debug("OnSuspending", "Prelaunch");

            // If the deferral is not obtained the suspension proceeds at the end of this method.
            // With the deferral there is still a 5 second time limit to completing suspension code.
            // The deferral allows code to be awaited in this method.
            var deferral = e.SuspendingOperation.GetDeferral();
            var completer = new TaskCompletionSource<object>();
            await Messenger.Instance.Suspending.Invoke(completer, true, true);
            await completer.Task;

            deferral.Complete();
        }

        /// <summary>
        ///     This only gets called if the application is activated via some special means. We are not currently doing so.
        /// </summary>
        /// <param name="args"></param>
        protected override void OnActivated(IActivatedEventArgs args)
        {
            switch (args.PreviousExecutionState)
            {
                case ApplicationExecutionState.ClosedByUser:
                case ApplicationExecutionState.NotRunning:
                    Toast.Debug("OnActivated", "NotRunning or ClosedByUser");
                    break;
                case ApplicationExecutionState.Running:
                    Toast.Debug("OnActivated", "Running");
                    break;
                case ApplicationExecutionState.Suspended:
                    Toast.Debug("OnActivated", "Suspended");
                    break;
                case ApplicationExecutionState.Terminated:
                    Toast.Debug("OnActivated", "Terminated");
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}