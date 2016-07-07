namespace Quickbird.Views
{
    using System.Threading.Tasks;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Navigation;
    using Util;
    using ViewModels;

    /// <summary>
    ///     The main shell that is always displayed when the user is signed in.
    ///     All daemons, local and internet network code starts and dies with this shell.
    /// </summary>
    public sealed partial class Shell
    {
        public Shell()
        {
            InitializeComponent();
            Bindings.Update();
        }

        public ShellViewModel ViewModel { get; private set; }

        /// <summary>
        ///     This is the true start-up for all the interesting parts of the program.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            // Can't allow back transitions to the landing or update pages.
            Frame.BackStack.Clear();
            ViewModel = new ShellViewModel(ContentFrame);
            Bindings.Update();

            var t = Task.Run(() => ((App) Application.Current).StartSession());
            ((App) Application.Current).AddSessionTask(t);

            Settings.Instance.CredsChanged += OnCredsChanged;
        }

        /// <summary>
        ///     Detects changes in roaming credentials and triggers a sign-out and sign-in.
        /// </summary>
        private void OnCredsChanged()
        {
            // Tigger a nav, OnNavigatedFrom() takes care of the rest.
            ((App) Application.Current).RootFrame.Navigate(typeof(SignOutView),
                SignOutView.ShouldItSignBackIn.YesSignBackInAgain);
        }

        /// <summary>
        /// Clean up and go back to starting 
        /// </summary>
        /// <param name="e"></param>
        protected override async void OnNavigatedFrom(NavigationEventArgs e)
        {
            Settings.Instance.CredsChanged -= OnCredsChanged;

            // Murders all the DashboardViewModels in a cascade (timers, event subs etc.).
            ViewModel.Kill();
            //Shutsdown the networking daemon code.
            await ((App) Application.Current).EndSession();
        }
    }
}