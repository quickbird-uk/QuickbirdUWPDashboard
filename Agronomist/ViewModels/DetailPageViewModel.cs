namespace Agronomist.ViewModels
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Windows.ApplicationModel;
    using Windows.UI.Xaml.Navigation;
    using Template10.Mvvm;
    using Template10.Services.NavigationService;

    public class DetailPageViewModel : ViewModelBase
    {
        private string _Value = "Default";

        public DetailPageViewModel()
        {
            if (DesignMode.DesignModeEnabled)
            {
                Value = "Designtime value";
            }
        }

        public string Value
        {
            get { return _Value; }
            set { Set(ref _Value, value); }
        }

        public override async Task OnNavigatedToAsync(object parameter, NavigationMode mode,
            IDictionary<string, object> suspensionState)
        {
            Value = suspensionState.ContainsKey(nameof(Value))
                ? suspensionState[nameof(Value)]?.ToString()
                : parameter?.ToString();
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
    }
}