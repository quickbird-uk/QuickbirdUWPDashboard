namespace Agronomist.ViewModels
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Windows.ApplicationModel;
    using Windows.UI.Xaml.Navigation;
    using Template10.Mvvm;
    using Template10.Services.NavigationService;

    internal class NewSitePageViewModel : ViewModelBase
    {
        private string _placeholder;

        public NewSitePageViewModel()
        {
            if (DesignMode.DesignModeEnabled)
            {
                Placeholder = "Designtime value";
            }
        }

        /// <summary>
        /// Sample property.
        /// </summary>
        public string Placeholder
        {
            get { return _placeholder; }
            set { Set(ref _placeholder, value); }
        }

        public override async Task OnNavigatedToAsync(object parameter, NavigationMode mode,
            IDictionary<string, object> suspensionState)
        {
            Placeholder = suspensionState.ContainsKey(nameof(Placeholder))
                ? suspensionState[nameof(Placeholder)]?.ToString()
                : parameter?.ToString();
            await Task.CompletedTask;
        }

        public override async Task OnNavigatedFromAsync(IDictionary<string, object> suspensionState, bool suspending)
        {
            if (suspending)
            {
                suspensionState[nameof(Placeholder)] = Placeholder;
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