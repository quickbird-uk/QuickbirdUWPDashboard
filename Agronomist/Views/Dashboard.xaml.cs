namespace Agronomist.Views
{
    using Windows.UI.Xaml.Controls;
    using Windows.UI.Xaml.Navigation;
    using ViewModels;

    public sealed partial class Dashboard : Page
    {
        public DashboardViewModel ViewModel;

        public Dashboard()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            var vm = e.Parameter as DashboardViewModel;
            ViewModel = vm;
            Bindings.Update();
        }
    }
}