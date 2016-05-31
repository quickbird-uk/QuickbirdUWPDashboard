namespace Agronomist.Views
{
    using Windows.UI.Xaml.Controls;
    using ViewModels;
    using System.Collections.Generic;
    using DatabasePOCOs.User;

    /// <summary>
    ///     An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class GraphingView : Page
    {
        public GraphingViewModel ViewModel = new GraphingViewModel();

        public GraphingView()
        {
            InitializeComponent();
           
        }

        private void CropCycleSelected(object sender, SelectionChangedEventArgs e)
        {
            ComboBox box = (ComboBox) sender;            
            KeyValuePair<CropCycle, string> selection = (KeyValuePair <CropCycle, string>)box.SelectedItem;
            ViewModel.SelectedCropCycle = selection.Key; 
        }
    }
}