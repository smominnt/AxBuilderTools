using System.Diagnostics;
using System.Windows;

namespace AxBuilder
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Page = new MainPage();
            MainFrame.Navigate(Page);
        }

        private async void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!await Page.SaveDialog())
            {
                e.Cancel = true;
            }
        }

        private void AboutButton_Handler(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                $"AX Builder version {Version}\n"
                + "Tool to build Autocross and other cone driving courses\n" 
                + "Created by: https://github.com/smominnt\n"
                + "Special thanks: Houston Region SCCA", 
                "About", MessageBoxButton.OK, MessageBoxImage.Information);
        }


        private void HelpButton_Handler(object sender, RoutedEventArgs e)
        {
            string url = "https://github.com/smominnt/AxBuilderTools";
            Process.Start(new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            });
        }

        private static readonly string Version = "1.0";

        readonly MainPage Page = null;
    }
}
