namespace Quickbird.Views
{
    using System;
    using System.Threading.Tasks;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;
    using Windows.UI.Xaml.Navigation;
    
    public sealed partial class SignOutView : Page
    {
        public SignOutView()
        {
            InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            var signInAgain = e.Parameter as bool? ?? false;

            //Hold this vire open for a minimum time to avoid flicker on fast syncs.
            var antiFlickerTimer = Task.Delay(TimeSpan.FromSeconds(5));

            await ((App) Application.Current).SignOut();
            if (signInAgain)
            {
                sign in here
            }
            await antiFlickerTimer;

            Frame.Navigate(typeof(LandingPage));
        }
    }
}