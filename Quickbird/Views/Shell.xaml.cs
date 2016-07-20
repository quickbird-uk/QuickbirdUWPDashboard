namespace Quickbird.Views
{
    using System.Threading.Tasks;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Navigation;
    using Util;
    using ViewModels;

    /// <summary>The main shell that is always displayed when the user is signed in. All daemons, local and
    /// internet network code starts and dies with this shell.</summary>
    public sealed partial class Shell
    {
        public Shell()
        {
            InitializeComponent();
            Bindings.Update();
        }

        public ShellViewModel ViewModel { get; private set; }

        /// <summary>Clean up and go back to starting</summary>
        /// <param name="e"></param>
        protected override async void OnNavigatedFrom(NavigationEventArgs e)
        {
            // Murders all the DashboardViewModels in a cascade (timers, event subs etc.).
            ViewModel.Kill();
            //Shutsdown the networking daemon code.
            await ((App) Application.Current).EndSession();
        }

        /// <summary>This is the true start-up for all the interesting parts of the program.</summary>
        /// <param name="e"></param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            // Can't allow back transitions to the landing or update pages.
            Frame.BackStack.Clear();
            ViewModel = new ShellViewModel(ContentFrame);
            Bindings.Update();

            ViewModel.FirstUpdate();

            var t = Task.Run(() => ((App) Application.Current).StartSession());
            ((App) Application.Current).AddSessionTask(t);
        }
    }
}
