namespace Agronomist
{
    using System;
    using System.Diagnostics;
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

    /// <summary>
    ///     Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : Application
    {
        private ExtendedExecutionSession _extendedExecutionSession;

        private Manager _networking;
        private Frame _rootFrame;

        /// <summary>
        ///     Initializes the singleton application object.  This is the first line of authored code
        ///     executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            InitializeComponent();
            Suspending += OnSuspending;
        }

        /// <summary>
        ///     Fired when the user attempts to open the program, even whent he program is already open.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
            // Only initialise if the program is not already open.
            if (_rootFrame == null)
            {
                // Creates a frame and shoves it in the provided default window.
                _rootFrame = Window.Current.Content as Frame;
                _rootFrame = new Frame();
                _rootFrame.NavigationFailed += OnNavigationFailed;
                Window.Current.Content = _rootFrame;

                using (var db = new MainDbContext())
                {
                    db.Database.Migrate();
                }

                _networking = new Manager();
                _networking.MqttDied += ResetNetworking;

                Window.Current.VisibilityChanged += OnVisibilityChanged;

            }

            // If the user launches the app when it is already open, just bring it to foreground.
            Window.Current.Activate();

            if (e.PrelaunchActivated)
            {
                Toast.Debug("Prelaunched", "");
            }
        }

        private void NavigateToInitialPage()
        {
            _rootFrame.Navigate(Settings.Instance.CredsSet ? typeof(Shell) : typeof(LandingPage));
            Window.Current.Activate();
        }

        private void ResetNetworking(Exception e)
        {
            Debug.WriteLine("MQTT died: " + e);
            _networking.MqttDied -= ResetNetworking;
            _networking = new Manager();
            _networking.MqttDied += ResetNetworking;
        }

        private void OnVisibilityChanged(object sender, VisibilityChangedEventArgs e)
        {
            Toast.Debug("OnVisibilityChanged", "");
            if (_rootFrame.Content == null)
            {
                NavigateToInitialPage();
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

        private async Task StartExtendedSession()
        {
            KillExtendedExecutionSession();

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
                KillExtendedExecutionSession();
                Toast.NotifyUserOfError("Windows error, program may fail to record, sync and alert when minimised.");
            }
        }

        private void KillExtendedExecutionSession()
        {
            if (_extendedExecutionSession != null)
            {
                _extendedExecutionSession.Revoked -= ExtendedExecutionRevoked;
                _extendedExecutionSession = null;
            }
        }

        private async void ExtendedExecutionRevoked(object sender, ExtendedExecutionRevokedEventArgs args)
        {
            if (args.Reason == ExtendedExecutionRevokedReason.Resumed)
            {
                await StartExtendedSession();
            }
            else if (args.Reason == ExtendedExecutionRevokedReason.SystemPolicy)
            {
                Toast.NotifyUserOfError(
                    "Program failing because there are too many apps running on this computer.");
            }
        }

        /// <summary>
        ///     Lifecycle suspend.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        public void OnSuspending(object sender, SuspendingEventArgs e)
        {
            Toast.Debug("Suspending", e.SuspendingOperation.ToString());

            // If the deferral is not obtained the suspension proceeds at the end of this method.
            // With the deferral there is still a 5 second time limit to completing suspension code.
            // The deferral allows code to be awaited in this method.
            var deferral = e.SuspendingOperation.GetDeferral();

            deferral.Complete();
        }

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