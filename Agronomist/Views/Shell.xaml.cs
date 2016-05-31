namespace Agronomist.Views
{
    using System.Collections.Generic;
    using System.Linq;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;
    using Windows.UI.Xaml.Navigation;
    using ViewModels;

    /// <summary>
    ///     An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Shell
    {
        public ShellViewModel ViewModel { get; }

        public Shell()
        {
            InitializeComponent();
            ViewModel = new ShellViewModel(ContentFrame);
            Bindings.Update();

        }
       
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            // Shell frame shouldn't have any backstack history.
            Frame.BackStack.Clear();
        }
    }
}