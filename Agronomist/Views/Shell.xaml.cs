namespace Agronomist.Views
{
    using System.Collections.Generic;
    using System.Linq;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;
    using Windows.UI.Xaml.Navigation;

    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Shell : Page
    {
        class CropRunInfo
        {
            public string CropName { get; set; }
            public string VarietyName { get; set; }
            public string PlantingDate { get; set; }
            public string BoxName { get; set; }
            public char IconLetter {
                get
                {
                    if(CropName != null)
                    {
                        return CropName.ToUpper().First();
                    }
                    else
                    {
                        return '֍';
                    }
                }
            }
            public bool IsCritical { get; set; }
            public Visibility IsVisible {
                get
                {
                    if(IsCritical)
                    {
                        return Visibility.Visible;
                    }
                    else
                    {
                        return Visibility.Collapsed;
                    }
                }
            }
        }

        private CropRunInfo selectedCrop
        {
            get
            {
                return (CropRunInfo) menu.SelectedItem;
            }
        }

        public Shell()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            // Shell frame shouldn't have any backstack history.
            Frame.BackStack.Clear();

            var CropRuns = new List<CropRunInfo>();

            CropRuns.Add(new CropRunInfo()
            {
                CropName = "Strawberry",
                VarietyName = "Weapon Grade",
                PlantingDate = "May 19",
                BoxName = "Box Two",
                IsCritical = false
            });

            CropRuns.Add(new CropRunInfo()
            {
                CropName = "Bananas",
                VarietyName = "Spanish",
                PlantingDate = "March 1",
                BoxName = "Box One",
                IsCritical = true
            });

            CropRuns.Add(new CropRunInfo()
            {
                CropName = "Pak Choi",
                VarietyName = "Pikachu",
                PlantingDate = "December 11",
                BoxName = "Box One",
                IsCritical = false
            });

            menu.ItemsSource = CropRuns;

            contentFrame.Navigate(typeof(Agronomist.Views.CropRunHome), this);
        }

        private void rocketButtonClicked(object sender, RoutedEventArgs e)
        {
            navigation.IsPaneOpen = !navigation.IsPaneOpen;
            if (!navigation.IsPaneOpen)
            {
                navigation.DisplayMode = SplitViewDisplayMode.CompactInline;
            }
        }

        private void showAlerts(object sender, RoutedEventArgs e)
        {
            notifications.IsPaneOpen = !notifications.IsPaneOpen;

            if (notifications.IsPaneOpen)
            {
                alertsButtonMessage.Text = "Hide \nAlerts";
            }
        }

        private void notificationsPaneClosing(SplitView sender, SplitViewPaneClosingEventArgs args)
        {
            alertsButtonMessage.Text = "Show \nAlerts";
        }

        private void UpdateContentFrame()
        {
            contentFrame.DataContext = selectedCrop;
        }

        private void CropSelected(object sender, SelectionChangedEventArgs e)
        {
            UpdateContentFrame();
        }
    }
}
