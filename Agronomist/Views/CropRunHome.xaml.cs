using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace TheDream.Views
{
    class ReadingCard
    {
        public string Reading { get; set; }
        public string ReadingType { get; set; }
        public string ReadingUnit { get; set; }
        public SolidColorBrush StatusColor { get; set; }
    }

    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class CropRunHome : Page
    {
        public CropRunHome()
        {
            this.InitializeComponent();

            List<ReadingCard> ReadingCards = new List<ReadingCard>();

            ReadingCards.Add(new ReadingCard()
            {
                Reading = "1.276",
                ReadingType = "Conductivity",
                ReadingUnit = "mS/cm",
                StatusColor = new SolidColorBrush(Colors.DarkOrange)
            });

            ReadingCards.Add(new ReadingCard()
            {
                Reading = "23.34",
                ReadingType = "Temperature",
                ReadingUnit = "° C",
                StatusColor = new SolidColorBrush(Colors.LimeGreen)
            });

            ReadingCards.Add(new ReadingCard()
            {
                Reading = "5.76",
                ReadingType = "Acidity",
                ReadingUnit = "pH",
                StatusColor = new SolidColorBrush(Colors.Gold)
            });

            cvsWaterTankReadings.Source = ReadingCards;
        }

        private void ReadingSelected(object sender, ItemClickEventArgs e)
        {

        }
    }
}
