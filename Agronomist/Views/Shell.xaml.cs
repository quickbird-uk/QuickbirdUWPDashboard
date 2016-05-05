namespace Agronomist.Views
{
    using Windows.UI.Xaml.Controls;
    using Template10.Controls;
    using Template10.Services.NavigationService;

    public sealed partial class Shell : Page
    {
        public Shell()
        {
            Instance = this;
            InitializeComponent();
        }

        public Shell(INavigationService navigationService) : this()
        {
            SetNavigationService(navigationService);
        }

        public static Shell Instance { get; set; }
        public static HamburgerMenu HamburgerMenu => Instance.HMenu;

        public void SetNavigationService(INavigationService navigationService)
        {
            HMenu.NavigationService = navigationService;
        }
    }
}