using System;
using System.IO;
using System.Windows;

namespace AxBuilder
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            AppDomain.CurrentDomain.UnhandledException += UnhandledExceptionHandler;
        }

        private void UnhandledExceptionHandler(object sender, UnhandledExceptionEventArgs args)
        {
            Exception ex = args.ExceptionObject as Exception;
            string text = "Unhandled exception:\n\n" + ((ex?.ToString()) ?? "null");

            try
            {
                MessageBox.Show(ex.Message, "Oops!", MessageBoxButton.OK, MessageBoxImage.Hand);
            }
            catch (Exception)
            {
            }

            try
            {
                File.WriteAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"crash_{DateTime.Now.Ticks}.txt"), text);
            }
            catch (Exception)
            {
            }

            Environment.Exit(1);
        }
    }


}
