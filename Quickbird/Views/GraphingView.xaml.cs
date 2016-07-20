namespace Quickbird.Views
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using Windows.UI;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;
    using Windows.UI.Xaml.Controls.Primitives;
    using Windows.UI.Xaml.Media;
    using Windows.UI.Xaml.Navigation;
    using DbStructure.User;
    using Syncfusion.UI.Xaml.Charts;
    using ViewModels;

    /// <summary>Never cahce this page, becuase it's view model does not refresh itself when new data comes
    /// in. The graphing code is a total mess</summary>
    public sealed partial class GraphingView : Page
    {
        private GraphingViewModel ViewModel;

        public GraphingView()
        {
            InitializeComponent();
            if (ViewModel == null)
            {
                ViewModel = new GraphingViewModel(ChartView.SuspendSeriesNotification,
                    ChartView.ResumeSeriesNotification);
            }
            ViewModel.SensorsToGraph.CollectionChanged += EditChart;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            ViewModel.Kill();
            ViewModel = null;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (ViewModel == null)
            {
                ViewModel = new GraphingViewModel(ChartView.SuspendSeriesNotification,
                    ChartView.ResumeSeriesNotification);
            }
        }

        private void AddToChart(GraphingViewModel.SensorTuple tuple)
        {
            ChartSeries chartSeries;
            //Water level sensor
            if (tuple.sensor.SensorTypeID == 19)
            {
                var series = new AreaSeries();
                series.Interior = new SolidColorBrush {Color = Colors.LightBlue, Opacity = 0.5};
                series.YBindingPath = "value";
                //By using the darkest of all values, we let other charts draw over the level area chart 
                series.CompositeMode = ElementCompositeMode.MinBlend;
                chartSeries = series;
            }
            else
            {
                //We could use bitmapLine series on really slow machines. 
                //It would be perfect for phones because they have very high DPI and the aliasing is less of an issue
                var series = new FastLineSeries();
                series.YBindingPath = "value";
                chartSeries = series;
            }
            chartSeries.ItemsSource = tuple.historicalDatapoints;
            chartSeries.EnableAnimation = true;
            chartSeries.AnimationDuration = TimeSpan.FromMilliseconds(200);
            chartSeries.XBindingPath = "timestamp";

            tuple.ChartSeries = chartSeries;
            tuple.Axis = DateAxis;
            chartSeries.IsSeriesVisible = false;

            //This is a string shortener! nothing else
            var placementNameLength = tuple.sensor.SensorType.Place.Name.Length > 6
                ? 6
                : tuple.sensor.SensorType.Place.Name.Length;
            var locationString = tuple.sensor.SensorType.Place.Name.Substring(0, placementNameLength);
            var spaceLocation = tuple.sensor.SensorType.Place.Name.IndexOf(' ');
            if (spaceLocation > 0 && tuple.sensor.SensorType.Place.Name.Length > spaceLocation + 1)
                locationString += tuple.sensor.SensorType.Place.Name.Substring(spaceLocation, 2) + ".";


            chartSeries.Label = tuple.sensor.SensorType.Param.Name + ": " + locationString;

            ChartView.Series.Add(chartSeries);
        }


        private void CropCycleSelected(object sender, SelectionChangedEventArgs e)
        {
            ChartView.SuspendSeriesNotification();
            // TODO: Kill this and move to Viewmodel.
            // THis is broken
            // You should two way bind box.selecteditem to ViewModel.SelectedCropCycle.
            var box = (ComboBox) sender;
            var selection = (KeyValuePair<CropCycle, string>) box.SelectedItem;
            ViewModel.SelectedCropCycle = selection.Key;
            StartDatePicker.Date = ViewModel.CycleStartTime;
            EndDatePicker.Date = ViewModel.CycleEndTime;

            ChartView.ResumeSeriesNotification();
        }

        private void EditChart(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (var item in e.NewItems)
                {
                    var tuple = item as GraphingViewModel.SensorTuple;
                    AddToChart(tuple);
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (var item in e.OldItems)
                {
                    var tuple = item as GraphingViewModel.SensorTuple;
                    ChartView.Series.Remove(tuple.ChartSeries);
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Replace || e.Action == NotifyCollectionChangedAction.Reset)
            {
                ChartView.Series.Clear();
                foreach (var sensorTuple in ViewModel.SensorsToGraph)
                {
                    AddToChart(sensorTuple);
                }
            }
        }

        private void EndDatePicker_DateChanged(CalendarDatePicker sender, CalendarDatePickerDateChangedEventArgs args)
        {
            if (args.NewDate.HasValue)
            {
                if (args.NewDate.Value.LocalDateTime.Date == ViewModel.CycleEndTime.Date)
                {
                    DateAxis.Maximum = ViewModel.CycleEndTime.LocalDateTime;
                }
                else
                {
                    DateAxis.Maximum = args.NewDate.Value.LocalDateTime.Date;
                }
                ViewModel.ChosenGraphPeriod = (DateTime) DateAxis.Maximum - (DateTime) DateAxis.Minimum;
            }
        }


        private void OnSensorToggleChecked(object sender, RoutedEventArgs e)
        {
            var button = sender as ToggleButton;
            var tuple = button.DataContext as GraphingViewModel.SensorTuple;
            tuple.visible = true;
        }

        private void OnSensorToggleUnchecked(object sender, RoutedEventArgs e)
        {
            var button = sender as ToggleButton;
            var tuple = button.DataContext as GraphingViewModel.SensorTuple;
            tuple.visible = false;
        }

        private void StartDatePicker_DateChanged(CalendarDatePicker sender, CalendarDatePickerDateChangedEventArgs args)
        {
            if (args.NewDate.HasValue)
            {
                if (args.NewDate.Value.LocalDateTime.Date > ViewModel.CycleStartTime.LocalDateTime)
                {
                    DateAxis.Minimum = args.NewDate.Value.LocalDateTime.Date;
                }
                else
                {
                    DateAxis.Minimum = ViewModel.CycleStartTime.LocalDateTime;
                }
                ViewModel.ChosenGraphPeriod = (DateTime) DateAxis.Maximum - (DateTime) DateAxis.Minimum;
            }
        }
    }
}
