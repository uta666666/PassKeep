using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using PassKeep.Material.View;
using PassKeep.Material.ViewModel;

namespace PassKeep.Material
{
    /// <summary>
    /// App.xaml の相互作用ロジック
    /// </summary>
    public partial class App : Application
    {
        private MainWindow _mainWindow;

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            Current.ShutdownMode = ShutdownMode.OnExplicitShutdown;

            LoginWindow loginWindow = new LoginWindow();
            if (loginWindow.ShowDialog() ?? false)
            {
                Current.ShutdownMode = ShutdownMode.OnMainWindowClose;

                //MainWindow mainWindow = new MainWindow();
                _mainWindow = new MainWindow();
                _mainWindow.Closing += _mainWindow_Closing;
                _mainWindow.Show();
            }
            else
            {
                Shutdown(0);
            }
        }

        private void _mainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var isLogout = ((sender as MainWindow).DataContext as MainViewModel).IsLogout;
            if (isLogout)
            {
                e.Cancel = true;
                (sender as MainWindow).Visibility = Visibility.Hidden;

                LoginWindow loginWindow = new LoginWindow();
                if (loginWindow.ShowDialog() ?? false)
                {
                    ((sender as MainWindow).DataContext as MainViewModel).IsLogout = false;
                    _mainWindow.Visibility = Visibility.Visible;
                }
                else
                {
                    Shutdown(0);
                }
            }
        }
    }
}
