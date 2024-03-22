using System.Configuration;
using System.Data;
using System.Windows;

namespace messenger
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Main(Object sender, StartupEventArgs e)
        {
            MainWindow mainWindow = new MainWindow(new ViewModels.MainViewModel(new Services.ConnectionService()));
            mainWindow.Title = "p2p messenger";
            mainWindow.Show();
        }
    }

}
