namespace Quickbird.Views
{
    using Windows.UI.Xaml.Controls;
    using Windows.UI.Xaml.Navigation;
    using ViewModels;

    public sealed partial class AddCropCycleView : Page
    {
        public AddCropCycleViewModel ViewModel = new AddCropCycleViewModel();


        public AddCropCycleView()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            ViewModel.Kill();
        }
    }
}