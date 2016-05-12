namespace Agronomist.ViewModels
{
    using System;
    using System.Collections.ObjectModel;
    using Windows.ApplicationModel;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;
    using Template10.Controls;
    using Template10.Mvvm;
    using Views;

    public class ShellViewModel : ViewModelBase
    {
        public ObservableCollection<HamburgerButtonInfo> SiteMenu = new ObservableCollection<HamburgerButtonInfo>();

        public ShellViewModel()
        {
            var button = CreateMenuEntry(Symbol.Add, "New Site", typeof(NewSitePage), "A");
            var button2 = CreateMenuEntry(Symbol.Add, "New Site 2", typeof(NewSitePage), "B");
            var button3 = CreateMenuEntry(Symbol.Add, "New Site 2", typeof(NewSitePage));

            SiteMenu.Add(button);
            SiteMenu.Add(button2);
            SiteMenu.Add(button3);

            if (DesignMode.DesignModeEnabled)
            {
            }
            else
            {
                
            }
        }

        private static HamburgerButtonInfo CreateMenuEntry(Symbol symbol, string buttonText, Type viewType, object parameter = null)
        {
            var stackPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal
            };
            stackPanel.Children.Add(new SymbolIcon
            {
                Width = 48,
                Height = 48,
                Symbol = symbol
            });
            stackPanel.Children.Add(new TextBlock
            {
                Margin = new Thickness(12, 0, 0, 0),
                VerticalAlignment = VerticalAlignment.Center,
                Text = buttonText
            });
            var button = new HamburgerButtonInfo
            {
                Content = stackPanel,
                ButtonType = HamburgerButtonInfo.ButtonTypes.Toggle,
                ClearHistory = false,
                PageType = viewType,
                PageParameter = parameter
            };
            return button;
        }
    }
}