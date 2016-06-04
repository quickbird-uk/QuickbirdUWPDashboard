namespace Agronomist.Views
{
    using Windows.UI.Xaml.Controls;
    using ViewModels;
    using System.Collections.Generic;
    using DatabasePOCOs.User;
    using Windows.UI.Xaml.Navigation;
    using Windows.UI.Xaml.Controls.Primitives;
    using DatabasePOCOs;
    using Syncfusion.UI.Xaml.Charts;
    using System;
    using System.Collections.Specialized;
    using Windows.UI.Xaml.Data;
    using Windows.UI.Xaml;

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
            ViewModel.SensorsToGraph.CollectionChanged += EditChart; 
           
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
            tuple.visible = true;
        }

        private void OnSensorToggleUnchecked(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            var button = sender as ToggleButton;
            var tuple = button.DataContext as GraphingViewModel.SensorTuple;
            tuple.visible = false;
        }

        private void EditChart(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (var item in e.NewItems)
                {
                    GraphingViewModel.SensorTuple tuple = item as GraphingViewModel.SensorTuple;
                    AddToChart(tuple);
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (var item in e.OldItems)
                {
                    GraphingViewModel.SensorTuple tuple = item as GraphingViewModel.SensorTuple;
                    ChartView.Series.Remove(tuple.realtimeChartSeries);
                    ChartView.Series.Remove(tuple.HistoricalChartSeries);
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Replace
                || e.Action == NotifyCollectionChangedAction.Reset)
            {
                ChartView.Series.Clear();
                foreach (var sensorTuple in ViewModel.SensorsToGraph)
                {
                    AddToChart(sensorTuple);
                }
            }
        }

        private void AddToChart(GraphingViewModel.SensorTuple tuple, bool historical = false)
        {
            var lineSeries = new FastLineSeries();
            lineSeries.ItemsSource = historical? tuple.HistoricalDatapoints : tuple.hourlyDatapoints;
            lineSeries.XBindingPath = "timestamp";
            lineSeries.YBindingPath = "value";
            tuple.realtimeChartSeries = lineSeries;
            lineSeries.IsSeriesVisible = false; 
            ChartView.Series.Add(lineSeries);
        }
    }
}