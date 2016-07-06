namespace Quickbird.Views
{
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Navigation;
    using ViewModels;

    /// <summary>
    ///     An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Shell
    {
        public Shell()
        {
            InitializeComponent();
            Bindings.Update();
        }

        public ShellViewModel ViewModel { get; private set; }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            // Can't allow back transitions to the landing or update pages.
            Frame.BackStack.Clear();
            ViewModel = new ShellViewModel(ContentFrame, Frame);
            Bindings.Update();
            ((App)Application.Current).StartSession();
        }

        protected override async void OnNavigatedFrom(NavigationEventArgs e)
        {
            await ((App)Application.Current).EndSession();
        }
    }
}