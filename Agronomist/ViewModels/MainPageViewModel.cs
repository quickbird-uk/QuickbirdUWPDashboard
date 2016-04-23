namespace Agronomist.ViewModels
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;
    using Windows.ApplicationModel;
    using Windows.UI.Xaml.Navigation;
    using Template10.Mvvm;
    using Template10.Services.NavigationService;
    using Views;

    public class MainPageViewModel : ViewModelBase
    {
        private string _value = "Gas";

        public MainPageViewModel()
        {
            if (DesignMode.DesignModeEnabled)
            {
                Value = "Designtime value";
            }
        }

        public string Value
        {
            get { return _value; }
            set { Set(ref _value, value); }
        }

        public override async Task OnNavigatedToAsync(object parameter, NavigationMode mode,
            IDictionary<string, object> suspensionState)
        {
            if (suspensionState.Any())
            {
                Value = suspensionState[nameof(Value)]?.ToString();
            }
            await Task.CompletedTask;
        }

        public override async Task OnNavigatedFromAsync(IDictionary<string, object> suspensionState, bool suspending)
        {
            if (suspending)
            {
                suspensionState[nameof(Value)] = Value;
            }
            await Task.CompletedTask;
        }

        public override async Task OnNavigatingFromAsync(NavigatingEventArgs args)
        {
            args.Cancel = false;
            await Task.CompletedTask;
        }

        public void GotoDetailsPage() =>
            NavigationService.Navigate(typeof(DetailPage), Value);

        public void GotoSettings() =>
            NavigationService.Navigate(typeof(SettingsPage), 0);

        public void GotoPrivacy() =>
            NavigationService.Navigate(typeof(SettingsPage), 1);

        public void GotoAbout() =>
            NavigationService.Navigate(typeof(SettingsPage), 2);

        public async void AuthReqTest()
        {
            string entryUrl = "https://ghapi46azure.azurewebsites.net/.auth/login/twitter";
            string resultUrl = "https://ghapi46azure.azurewebsites.net/.auth/login/done";
            var cred = await NetLib.Creds.FromBroker(entryUrl, resultUrl);
            string baseUrl = "https://ghapi46azure.azurewebsites.net/api";
            string tableName = "People";
            var response = await NetLib.Request.RequestTable(baseUrl, tableName, cred);
            Debug.WriteLine(response);
        }
    }
}