namespace Agronomist.Views
{
    using Windows.UI.Xaml.Controls;
    using ViewModels;
    using System.Collections.Generic;
    using DatabasePOCOs.User;
    using Windows.UI.Xaml.Navigation;
    using Windows.UI.Xaml.Controls.Primitives;
    using DatabasePOCOs;

    /// <summary>
    ///     An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class GraphingView : Page
    {
        public GraphingViewModel ViewModel = new GraphingViewModel();

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

        }

        public GraphingView()
        {
            InitializeComponent();
           
        }

        private void CropCycleSelected(object sender, SelectionChangedEventArgs e)
        {
            // TODO: Kill this and move to Viewmodel.
            // THis is broken
            // You should two way bind box.selecteditem to ViewModel.SelectedCropCycle.
            ComboBox box = (ComboBox) sender;            
            KeyValuePair<CropCycle, string> selection = (KeyValuePair <CropCycle, string>)box.SelectedItem;
            ViewModel.SelectedCropCycle = selection.Key; 
        }

        private void OnSensorToggleChecked(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            var button = sender as ToggleButton;
            var tuple = button.DataContext as GraphingViewModel.SensorTuple;
            ViewModel.SensorsToGraph.Add(tuple);
        }

        private void OnSensorToggleUnchecked(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            var button = sender as ToggleButton;
            var tuple = button.DataContext as GraphingViewModel.SensorTuple;
            ViewModel.SensorsToGraph.Remove(tuple);
        }
    }
}