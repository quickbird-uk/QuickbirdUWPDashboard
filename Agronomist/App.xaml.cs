namespace Agronomist
{
    using System;
    using System.Diagnostics;
    using System.Threading;
    using Windows.ApplicationModel;
    using Windows.ApplicationModel.Activation;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;
    using Windows.UI.Xaml.Navigation;
    using Microsoft.EntityFrameworkCore;
    using Models;
    using Util;
    using Windows.UI.Core;
    using Windows.ApplicationModel.ExtendedExecution;
    using System.Threading.Tasks;
    using LocalNetworking;

    /// <summary>
    ///     Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : Application
    {

        private LocalNetworking.Manager _networking;
        ExtendedExecutionSession _extendedExecutionSession; 

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
        ///     Invoked when the application is launched normally by the end user.  Other entry points
        ///     will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
           
            Toast.Debug("OnLaunched", e.Kind.ToString());

            var rootFrame = Window.Current.Content as Frame;
            
            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();

                rootFrame.NavigationFailed += OnNavigationFailed;

                _networking = new Manager();
                _networking.MqttDied += ResetNetworking;


                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: Load state from previously suspended application
                }

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;

            }

            Window.Current.VisibilityChanged += OnVisibilityChanged; 

            if (e.PrelaunchActivated == false)
            {
                using (var db = new MainDbContext())
                {
                    db.Database.Migrate();
                }

                if (rootFrame.Content == null)
                {
                    // When the navigation stack isn't restored navigate to the first page,
                    // configuring the new page by passing required information as a navigation
                    // parameter
                    
                    if (Settings.Instance.CredsSet)
                    {
                        rootFrame.Navigate(typeof(Views.Shell), e.Arguments);
                    }
                    else
                    {
                        rootFrame.Navigate(typeof(Views.LandingPage), e.Arguments);
                    }

                }
                // Ensure the current window is active
                Window.Current.Activate();
            }
        }

        private void ResetNetworking(Exception e)
        {
            Debug.WriteLine("MQTT died: " + e.ToString());
            _networking.MqttDied -= ResetNetworking;
            _networking = new Manager();
            _networking.MqttDied += ResetNetworking;
        }

        private void OnVisibilityChanged(object sender, VisibilityChangedEventArgs e)
        {
            
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

            _extendedExecutionSession = new ExtendedExecutionSession()
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
        ///     Invoked when application execution is being suspended.  Application state is saved
        ///     without knowing whether the application will be terminated or resumed with the contents
        ///     of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private async void OnSuspending(object sender, SuspendingEventArgs e)
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