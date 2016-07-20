namespace Quickbird.Views
{
    using Windows.UI.Xaml.Controls;
    using ViewModels;

    /// <summary>An empty page that can be used on its own or navigated to within a Frame.</summary>
    public sealed partial class LandingPage : Page
    {
        public LandingPage() { InitializeComponent(); }

        public LandingPageViewModel ViewModel { get; } = new LandingPageViewModel();
    }
}
