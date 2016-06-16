namespace Agronomist.Views
{
    using Windows.UI.Xaml.Controls;
    using Windows.UI.Xaml.Navigation;
    using ViewModels;
    
    public sealed partial class CropView : Page
    {
        public CropViewModel ViewModel;

        public CropView()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            var sharedViewModel = e.Parameter as SharedCropRunViewModel;
            ViewModel = new CropViewModel(ContentFrame, sharedViewModel);
            Bindings.Update();
        }
    }
}