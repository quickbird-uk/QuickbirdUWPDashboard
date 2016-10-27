namespace Quickbird.Views
{
    using System;
    using Windows.UI.Xaml.Controls;
    using ViewModels;
    using Windows.UI.Xaml.Navigation;

    /// <summary>An empty page that can be used on its own or navigated to within a Frame.</summary>
    public sealed partial class RegisterView : Page
    {
        public RegisterView() { InitializeComponent(); }

        public RegisterViewModel ViewModel { get; } = new RegisterViewModel();

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            var emailPwd = e.Parameter as Tuple<string, string>;
            if (emailPwd != null)
            {
                ViewModel.Email = emailPwd.Item1;
                ViewModel.Password = emailPwd.Item2;
            }
            base.OnNavigatedTo(e);
        }
    }
}
