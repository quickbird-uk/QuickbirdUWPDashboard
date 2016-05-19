namespace Agronomist.Views
{
    using Windows.UI.Xaml.Controls;
    using Microsoft.EntityFrameworkCore;
    using Models;
    using ViewModels;

    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            ViewModel = new MainPageViewModel();
            DataContext = new MainPageViewModel();
            InitializeComponent();
        }

        public MainPageViewModel ViewModel { get; set; }
    }
}