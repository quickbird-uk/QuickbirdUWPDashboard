namespace Quickbird.Views
{
    using System;
    using Windows.UI.Xaml.Controls;
    using Windows.UI.Xaml.Navigation;
    using ViewModels;

    public sealed partial class CropView : Page
    {
        public CropViewModel ViewModel;

        public CropView()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            ViewModel = e.Parameter as CropViewModel;
            if (null == ViewModel)
                throw new ArgumentException("Tried to navigate to CropView without sending a ViewModel.");
            ViewModel.SetContentFrame(ContentFrame);
            Bindings.Update();
        }
    }
}