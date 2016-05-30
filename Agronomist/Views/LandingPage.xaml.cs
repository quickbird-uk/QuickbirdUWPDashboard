namespace Agronomist.Views
{
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;

    /// <summary>
    ///     An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class LandingPage : Page
    {
        public LandingPage()
        {
            InitializeComponent();
        }

        private void Authenticate(object sender, RoutedEventArgs e)
        {
            var isAuthenticated = true;

            if (isAuthenticated)
            {
                Frame.Navigate(typeof(Shell));
            }
        }
    }
}