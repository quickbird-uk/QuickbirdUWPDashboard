namespace Agronomist
{
    using System.Threading.Tasks;
    using Windows.ApplicationModel.Activation;
    using Windows.UI.Xaml;
    using Models;
    using Services.SettingsServices;
    using Template10.Common;
    using Template10.Controls;
    using Views;

    /// Documentation on APIs used in this page:
    /// https://github.com/Windows-XAML/Template10/wiki
    sealed partial class App : BootStrapper
    {
        public App()
        {
            InitializeComponent();
            SplashFactory = e => new Splash(e);

            var settings = SettingsService.Instance;
            RequestedTheme = settings.AppTheme;
            CacheMaxDuration = settings.CacheMaxDuration;
            ShowShellBackButton = settings.UseShellBackButton;
        }

        public override async Task OnInitializeAsync(IActivatedEventArgs args)
        {
            if (!(Window.Current.Content is ModalDialog))
            {
                // create a new frame 
                var nav = NavigationServiceFactory(BackButton.Attach, ExistingContent.Include);
                // create modal root
                Window.Current.Content = new ModalDialog
                {
                    DisableBackButtonWhenModal = true,
                    Content = new Shell(nav),
                    ModalContent = new Busy()
                };
            }
            await Task.CompletedTask;
        }

        public override async Task OnStartAsync(StartKind startKind, IActivatedEventArgs args)
        {
            // long-running startup tasks go here

            InitialiseDatabase();

            NavigationService.Navigate(typeof(MainPage));
            await Task.CompletedTask;
        }

        private void InitialiseDatabase()
        {
            MainDbContext x = new MainDbContext();
            var y = x.CropCycles;
        }
    }
}