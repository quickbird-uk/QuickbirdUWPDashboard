namespace Agronomist.ViewModels
{
    using System.Linq;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;
    using Views;

    public class ShellViewModel : ViewModelBase
    {


        private void Update()
        {
            
        }

        public void NavToGraphingView()
        {
            
            ((Frame) Window.Current.Content).Navigate(typeof(GraphingView));
        }
        
    }
}