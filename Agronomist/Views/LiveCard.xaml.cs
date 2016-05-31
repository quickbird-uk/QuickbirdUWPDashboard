// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Agronomist.Views
{
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;

    public sealed partial class LiveCard : UserControl
    {
        public LiveCard()
        {
            this.InitializeComponent();
        }

        private void adjustButtonChecked(object sender, RoutedEventArgs e)
        {
            infoPanel.Visibility = Visibility.Collapsed;
        }

        private void adjustButtonUnchecked(object sender, RoutedEventArgs e)
        {
            infoPanel.Visibility = Visibility.Visible;
        }
    }
}
