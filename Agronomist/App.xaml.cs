namespace Agronomist
{
    using System.Threading.Tasks;
    using Windows.ApplicationModel.Activation;
    using Windows.UI.Xaml;
    using Microsoft.Data.Entity;
    using Models;
    using Services.SettingsServices;
    using Template10.Common;
    using Template10.Controls;
    using Views;

    sealed partial class App : BootStrapper
    {
        public App()
        {
            InitializeComponent();
            SplashFactory = e => new Splash(e);

            var settings = SettingsService.Instance;
            RequestedTheme = settings.AppTheme;
            CacheMaxDuration = settings.CacheMaxDuration;
            // Shell back button is a bad idea in an app that likes to hife the window chrtom altogether.
            ShowShellBackButton = false;
        }


        public override async Task OnInitializeAsync(IActivatedEventArgs args)
        {
            // Code here is sued to replace the default root-frame of the Nav service with a shell including the hamburger menu.
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

        /// <summary>
        ///     Used to execute long running startup things.
        ///     Is only run when starting up the app fresh, not when restoring.
        /// </summary>
        /// <param name="startKind"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public override async Task OnStartAsync(StartKind startKind, IActivatedEventArgs args)
        {
            // Long-running startup tasks go here.
            await InitialiseDatabase();
            NavigationService.Navigate(typeof(MainPage));
            await Task.CompletedTask;
        }

        /// <summary>
        ///     Runs any database initialisation and maintenance tasks that must be completed before the app starts.
        /// </summary>
        /// <returns>Awaitable task.</returns>
        private async Task InitialiseDatabase()
        {
            // Make sure the database is created and migrated up-to-date.
            await Task.Run(() =>
            {
                using (var x = new MainDbContext())
                    x.Database.Migrate();
            });
        }
    }
}