namespace Agronomist.Views
{
    using Windows.UI.Xaml.Controls;
    using Microsoft.Data.Entity;
    using Models;

    /// <summary>
    ///     An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            InitializeComponent();
            using (var db = new MainDbContext())
            {
                db.Database.Migrate();
            }
        }
    }
}