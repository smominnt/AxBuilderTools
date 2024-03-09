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

        readonly MainPage Page = null;
    }
}
