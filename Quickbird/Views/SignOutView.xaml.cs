namespace Quickbird.Views
{
    using System;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;
    using Windows.UI.Xaml.Navigation;
    using JetBrains.Annotations;

    /// <summary>
    ///     Signs out then navigates to Landing OR re-signs in and goes back to shell (requires parameter).
    /// </summary>
    public sealed partial class SignOutView : Page, INotifyPropertyChanged
    {
        public enum ShouldItSignBackIn
        {
            YesSignBackInAgain,
            DontSignBackInAgain
        }

        private string _currentOperation = "Signing Out";

        public SignOutView()
        {
            InitializeComponent();
        }

        /// <summary>
        ///     UI information for the user.
        /// </summary>
        public string CurrentOperation
        {
            get { return _currentOperation; }
            set
            {
                if (value == _currentOperation) return;
                _currentOperation = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            // Default to don't sign back in again.
            var signInAgain = e.Parameter as ShouldItSignBackIn? ?? ShouldItSignBackIn.DontSignBackInAgain;

            var antiFlickerDelay = Task.Delay(TimeSpan.FromSeconds(4)); //The other delays do not add to this.

            await ((App) Application.Current).SignOut();

            await Task.Delay(TimeSpan.FromSeconds(2)); // Show sign out for minimum 2 secs

            Type navPage;

            if (signInAgain == ShouldItSignBackIn.YesSignBackInAgain)
            {
                CurrentOperation = "Signing in with new credentials";
                await Task.Delay(TimeSpan.FromSeconds(2)); // Show sign in for minimum 2 secs

                navPage = typeof(Shell);
            }
            else
            {
                navPage = typeof(LandingPage);
            }

            await antiFlickerDelay;

            Frame.Navigate(navPage);
        }

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}