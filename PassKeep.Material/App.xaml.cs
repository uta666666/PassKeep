using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using PassKeep.Material.View;

namespace PassKeep.Material
{
    /// <summary>
    /// App.xaml の相互作用ロジック
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            Current.ShutdownMode = ShutdownMode.OnExplicitShutdown;

            LoginWindow loginWindow = new LoginWindow();
            if (loginWindow.ShowDialog() ?? false)
            {
                Current.ShutdownMode = ShutdownMode.OnMainWindowClose;

                MainWindow mainWindow = new MainWindow();
                mainWindow.Show();
            }
            else
            {
                Shutdown(0);
            }
        }
    }
}
