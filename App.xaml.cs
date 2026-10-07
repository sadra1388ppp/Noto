using System;
using System.Windows;

namespace SimpleReminders;

public partial class App : Application
{
    private void App_Startup(object sender, StartupEventArgs e)
    {
        try
        {
            var window = new MainWindow();
            MainWindow = window;
            window.Show();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                "Noto could not start.\n\n" +
                ex.Message +
                "\n\n" +
                ex.InnerException?.Message,
                "Noto",
                MessageBoxButton.OK,
                MessageBoxImage.Error);

            Shutdown(-1);
        }
    }
}
