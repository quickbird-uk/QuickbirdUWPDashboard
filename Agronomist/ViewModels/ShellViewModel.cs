namespace Agronomist.ViewModels
{
    using System.Linq;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;
    using Views;

    public class ShellViewModel : ViewModelBase
    {
        private readonly Frame _contentFrame;

        public ShellViewModel(Frame contentFrame)
        {
            _contentFrame = contentFrame;
        }

        private void Update()
        {
            
        }

        public void NavToGraphingView()
        {
            _contentFrame.Navigate(typeof(GraphingView));
        }
    }
}