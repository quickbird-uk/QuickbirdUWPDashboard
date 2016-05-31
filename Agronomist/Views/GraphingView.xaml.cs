namespace Agronomist.Views
{
    using Windows.UI.Xaml.Controls;
    using ViewModels;

    /// <summary>
    ///     An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class GraphingView : Page
    {
        public GraphingViewModel ViewModel = new GraphingViewModel();

        public GraphingView()
        {
            InitializeComponent();
            
        }
    }
}