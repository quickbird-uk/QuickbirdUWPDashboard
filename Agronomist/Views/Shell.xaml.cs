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

        private CropRunInfo SelectedCrop => (CropRunInfo) Menu.SelectedItem;

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            // Shell frame shouldn't have any backstack history.
            Frame.BackStack.Clear();

            var cropRuns = new List<CropRunInfo>
            {
                new CropRunInfo
                {
                    CropName = "Strawberry",
                    VarietyName = "Weapon Grade",
                    PlantingDate = "May 19",
                    BoxName = "Box Two",
                    IsCritical = false
                },
                new CropRunInfo
                {
                    CropName = "Bananas",
                    VarietyName = "Spanish",
                    PlantingDate = "March 1",
                    BoxName = "Box One",
                    IsCritical = true
                },
                new CropRunInfo
                {
                    CropName = "Pak Choi",
                    VarietyName = "Pikachu",
                    PlantingDate = "December 11",
                    BoxName = "Box One",
                    IsCritical = false
                }
            };




            Menu.ItemsSource = cropRuns;

            ContentFrame.Navigate(typeof(CropRunHome), this);
        }

        private void RocketButtonClicked(object sender, RoutedEventArgs e)
        {
            Navigation.IsPaneOpen = !Navigation.IsPaneOpen;
            if (!Navigation.IsPaneOpen)
            {
                Navigation.DisplayMode = SplitViewDisplayMode.CompactInline;
            }
        }
        
    

        private void UpdateContentFrame()
        {
            ContentFrame.DataContext = SelectedCrop;
        }

        private void CropSelected(object sender, SelectionChangedEventArgs e)
        {
            UpdateContentFrame();
        }

        private class CropRunInfo
        {
            public string CropName { get; set; }
            public string VarietyName { get; set; }
            public string PlantingDate { get; set; }
            public string BoxName { get; set; }

            public char IconLetter
            {
                get
                {
                    if (CropName != null)
                    {
                        return CropName.ToUpper().First();
                    }
                    return '֍';
                }
            }

            public bool IsCritical { get; set; }

            public Visibility IsVisible
            {
                get
                {
                    if (IsCritical)
                    {
                        return Visibility.Visible;
                    }
                    return Visibility.Collapsed;
                }
            }
        }
    }
}