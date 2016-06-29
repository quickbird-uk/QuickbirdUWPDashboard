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
using System.Collections.ObjectModel;
using DbStructure;
using DbStructure.User;
using DbStructure.Global; 

namespace Quickbird.Views
{
    using System.Globalization;
    using System.Threading.Tasks;

    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class AnalyticsMode : Page
    {
        public List<KeyValuePair<Guid, List<KeyValuePair<DateTime, double>>>> AllDataSinceStartOfCropRun;
        public Dictionary<Guid, Syncfusion.UI.Xaml.Charts.FastLineSeries> ChartDictionary = 
            new Dictionary<Guid, Syncfusion.UI.Xaml.Charts.FastLineSeries>();

        public List<Sensor> SelectedSensors = new List<Sensor>();
        public CropCycle SelectedCropRun;

        public AnalyticsMode()
        {
            //this.InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            //base.OnNavigatedTo(e);
            //Frame.Margin = new Thickness(0, 0, 0, 0);
            //PageGrid.DataContext = ViewModel.ViewModel.Singleton.Room;
            //CropRunComboBox.SelectedIndex = 0;
            //SelectedCropRun = CropRunComboBox.SelectedItem as Cycle;

            //var startDate = SelectedCropRun.StartDate ?? DateTime.Now;
            //AllDataSinceStartOfCropRun = (await ViewModel.ViewModel.Singleton.GetPastData(startDate, DateTime.Now)).ToList();
        }

        private void OnCloseButtonClick(object sender, RoutedEventArgs e)
        {
            Frame.Margin = new Thickness(320, 0, 0, 120);
            Frame.GoBack();
        }

        private void OnSensorToggleChecked(object sender, RoutedEventArgs e)
        {
            var button = sender as ToggleButton;
            var sensor = button.DataContext as Sensor;
            SelectedSensors.Add(sensor);
            AddToChart(sensor);
        }

        private void OnSensorToggleUnchecked(object sender, RoutedEventArgs e)
        {
            var button = sender as ToggleButton;
            var sensor = button.DataContext as Sensor;
            SelectedSensors.Remove(sensor);
            RemoveFromChart(sensor);
        }

        private void ExportToExcel(object sender, RoutedEventArgs e)
        {
             
        }

        private void OnCropRunSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //SelectedCropRun = CropRunComboBox.SelectedItem as Cycle;

            //if (SelectedCropRun != null)
            //{
            //    StartDatePicker.Date = SelectedCropRun.StartDate ?? DateTime.Now;
            //    StartDatePicker.MinYear = SelectedCropRun.StartDate ?? DateTime.Now;
            //    StartDatePicker.MaxYear = SelectedCropRun.EndDate ?? DateTime.Now;

            //    EndDatePicker.Date = SelectedCropRun.EndDate ?? DateTime.Now;
            //    EndDatePicker.MinYear = SelectedCropRun.StartDate ?? DateTime.Now;
            //    EndDatePicker.MaxYear = SelectedCropRun.EndDate ?? DateTime.Now;
            //}
        }

        private void AddToChart(Sensor sensor)
        {
            //var dataset = from sensorData in AllDataSinceStartOfCropRun
            //              where sensorData.Key == sensor.ID
            //              select sensorData.Value;

            //var lineSeries = new Syncfusion.UI.Xaml.Charts.FastLineSeries();
            //lineSeries.ItemsSource = dataset.First();
            //lineSeries.XBindingPath = "Key";
            //lineSeries.YBindingPath = "Value";
            //lineSeries.Label = sensor.Device.Placement.Name;
            //lineSeries.EnableAnimation = true;
            //lineSeries.AnimationDuration = new TimeSpan(0,0,2);
            //ChartDictionary.Add(sensor.ID, lineSeries);
            //ChartView.Series.Add(lineSeries);
        }

        private void RemoveFromChart(Sensor sensor)
        {
            //var lineSeries = from Pair in ChartDictionary
            //                 where Pair.Key == sensor.ID
            //                 select Pair.Value;

            //foreach (var series in lineSeries.ToList())
            //{
            //    ChartView.Series.Remove(series);
            //    ChartDictionary.Remove(sensor.ID);
            //}
        }

        private void OnStartDateChanged(object sender, DatePickerValueChangedEventArgs e)
        {
            //DateAxis.Minimum = new DateTime(e.NewDate.Ticks + StartTimePicker.Time.Ticks);
        }

        private void OnEndDateChanged(object sender, DatePickerValueChangedEventArgs e)
        {
            //DateAxis.Maximum = new DateTime(e.NewDate.Ticks + EndTimePicker.Time.Ticks);
        }

        private void OnStartTimeChanged(object sender, TimePickerValueChangedEventArgs e)
        {
            //DateAxis.Minimum = new DateTime(e.NewTime.Ticks + StartDatePicker.Date.Ticks);
        }

        private void OnEndTimeChanged(object sender, TimePickerValueChangedEventArgs e)
        {
            //DateAxis.Maximum = new DateTime(e.NewTime.Ticks + EndDatePicker.Date.Ticks);
        }

        /// <summary>
        /// Can be used before ExportDataToCSV in order to filter the data down.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="sensors"></param>
        /// <returns></returns>
        private static IEnumerable<KeyValuePair<Guid, List<KeyValuePair<DateTime, double>>>> FilterDataQuery(IEnumerable<KeyValuePair<Guid, List<KeyValuePair<DateTime, double>>>> data,
            DateTimeOffset start, DateTimeOffset end, IEnumerable<Guid> sensors)
        {
            // Filter sensors first, it is the least intensive part.
            var senFiltered = sensors == null ? data : data.Where(x => sensors.Contains(x.Key));
            foreach (var item in senFiltered)
            {
                var dateFiltered = item.Value.Where(d => d.Key >= start && d.Key <= end).ToList();
                yield return new KeyValuePair<Guid, List<KeyValuePair<DateTime, double>>>(item.Key, dateFiltered);
            }
        }

        /// <summary>
        ///     Creates a enumerable of string that represent a CSV file. This should return instantly, the result is a very long lazy linq query.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private static IEnumerable<string> ExportDataToCSVQuery(IEnumerable<KeyValuePair<Guid, List<KeyValuePair<DateTime, double>>>> data)
        {
            // Get the sensor data so that the titles can be extracted.
            //var room = ViewModel.ViewModel.Singleton.Room;
            //var withSensors = data.Select(x => new { sensor = room.Sensors.First(s => s.ID == x.Key), x.Value });

            //// Create the row of titles ordered by placement and sensor.
            //var ordered = 
            //    from item in withSensors
            //    orderby item.sensor.Placement.Name, item.sensor.Name
            //    select item;
            //var titles =
            //    string.Join(",", (from item in ordered
            //        let s = item.sensor
            //        select $"{s.Name}({s.Units})-{s.Type}-{s.SensorNumber}-{s.Device.Name}-{s.Placement.Name}"));

            //// Flatten the collection so that it can be grouped by time instead of sensor.
            //var flatten =
            //    ordered.SelectMany(x => x.Value.Select(v => new {x.sensor, date = v.Key, value = v.Value}));

            //// Re-group by datetime so that the readings from the same time are on the same row, then order by time.
            //var grouped =
            //    from item in flatten
            //    group item by item.date
            //    into dateGroups
            //    orderby dateGroups.Key
            //    select dateGroups;
            //var ic = CultureInfo.InvariantCulture;

            //// Using invariant culture is essential for changing a time or number to string.
            //// Transform each group of values into a row of data, sorting in the same order as the titles.
            //var datalines =
            //    from g in grouped
            //    let sorted = from inner in g orderby inner.sensor.Placement.Name, inner.sensor.Name select inner.value.ToString(ic)
            //    select $"{g.Key.ToString(ic)},{string.Join(",", sorted)}";

            //// Tack the titles row onto the front and return.
            //return new[] {titles}.Concat(datalines);

            return new string[2]; 
        }

        private static async Task SaveFile(IEnumerable<string> data)
        {
            var saver = new Windows.Storage.Pickers.FileSavePicker
            {
                SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary
            };
            saver.FileTypeChoices.Add("CSV", new List<string>() {".csv"});
            var file = await saver.PickSaveFileAsync();

        }
    }
}
