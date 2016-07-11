// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Quickbird.Views
{
    using System;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;
    using Windows.UI.Xaml.Navigation;
    using ViewModels;

    /// <summary>An empty page that can be used on its own or navigated to within a Frame.</summary>
    public sealed partial class AddYieldView : Page
    {
        public AddYieldViewModel ViewModel;

        public AddYieldView() { InitializeComponent(); }

        protected override void OnNavigatedFrom(NavigationEventArgs e) { ViewModel.Kill(); }


        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is Guid)
            {
                ViewModel = new AddYieldViewModel((Guid) e.Parameter);
            }
        }

        private async void AddYieldAndOrEndRunClick(object sender, RoutedEventArgs e)
        {
            await ViewModel.SaveCropRun();
            Frame.GoBack();
        }

        private void CancelClick(object sender, RoutedEventArgs e) { Frame.GoBack(); }

        private void TextBox_TextChanging(TextBox sender, TextBoxTextChangingEventArgs args)
        {
            ViewModel.UserEnteredText = sender.Text;
        }
    }
}
