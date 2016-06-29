using Quickbird.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Quickbird.Views
{
    using ViewModels;

    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class AddYieldView : Page
    {
        public AddYieldViewModel ViewModel = null; 

        public AddYieldView()
        {
            this.InitializeComponent();
        }


        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is Guid)
            {
                ViewModel = new AddYieldViewModel((Guid)e.Parameter);
            }
        }

        private void TextBox_TextChanging(TextBox sender, TextBoxTextChangingEventArgs args)
        {
            ViewModel.UserEnteredText = sender.Text;
            
        }

        private async void AddYieldAndOrEndRunClick(object sender, RoutedEventArgs e)
        {
            await ViewModel.SaveCropRun(); 
            this.Frame.GoBack();
        }

        private void CancelClick(object sender, RoutedEventArgs e)
        {
            this.Frame.GoBack();
        }
    }
}
