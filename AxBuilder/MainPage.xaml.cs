using ModernWpf.Controls;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace AxBuilder
{
    /// <summary>
    /// Interaction logic for MainPage.xaml
    /// </summary>
    public partial class MainPage : ModernWpf.Controls.Page
    {
        public MainPage()
        {
            InitializeComponent();
            ResetApplication();
        }

        private void ChangeMainWindowTitleAndText(string filename = "")
        {
            TrackFilename = filename;
            Window mainWindow = Application.Current.MainWindow;
            var title = string.IsNullOrEmpty(TrackFilename) ? "AX Builder" : $"AX Builder - {TrackFilename}";

            if (mainWindow != null)
            {
                mainWindow.Title = title;
                (mainWindow.FindName("FileTitle") as TextBlock).Text = title;
            }
        }

        internal void ResetApplication()
        {
            // Clear data
            StartGrid = null;
            StartLine = null;
            FinishLine = null;
            ImageLocation = null;
            TrackFilename = string.Empty;
            ParsedPixels = 0;
            ParsedDistance = 0;
            IsImperial = true;
            IsChanged = false;
            MyCanvas.Children.Clear();


            // Disable buttons
            NewButton.IsEnabled = false;
            NewButton.Opacity = 0.5;
            SaveButton.IsEnabled = false;
            SaveButton.Opacity = 0.5;
            BuildButton.IsEnabled = false;
            BuildButton.Opacity = 0.5;

            // Clear UI elements
            MyImage.Source = null;
            CurrentLine = null;
            IsManipulating = false;
            ScaleStatus.IsChecked = false;
            IsMiddleButtonPressed = false;
            Distance.Text = "0";
            Pixels.Text = "0 px";


            // Set default tool
            UprightButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            ChangeMainWindowTitleAndText();

            GC.Collect();
        }


        // ------------------------------------------------------
        // Data
        // ------------------------------------------------------

        private Viewbox StartGrid { get; set; }

        private Viewbox StartLine { get; set; }

        private Viewbox FinishLine { get; set; }

        private string ImageLocation { get; set; }

        private string TrackFilename { get; set; }

        private double ParsedPixels { get; set; }

        private double ParsedDistance { get; set; }

        private bool IsImperial { get; set; }

        private bool IsChanged { get; set; }



        // ------------------------------------------------------
        // UI helpers
        // ------------------------------------------------------

        private Line CurrentLine;

        private Point OriginalPointerPosition;

        private TranslateTransform Transform = new TranslateTransform();

        private bool IsManipulating = false;

        private bool IsMiddleButtonPressed = false;

        private AppBarToggleButton ActiveButton;

        private static readonly BitmapImage StandConeImage =
            new BitmapImage(new Uri("pack://application:,,,/Assets/ui_cone.png"));

        private static readonly BitmapImage PointConeImage =
            new BitmapImage(new Uri("pack://application:,,,/Assets/ui_cone_point.png"));

        private static readonly BitmapImage LyingConeImage =
            new BitmapImage(new Uri("pack://application:,,,/Assets/ui_cone_lying.png"));

        private static readonly BitmapImage CarImage =
            new BitmapImage(new Uri("pack://application:,,,/Assets/ui_car.png"));

        private static readonly BitmapImage StartConeImage =
            new BitmapImage(new Uri("pack://application:,,,/Assets/ui_cone_start.png"));

        private static readonly BitmapImage FinishConeImage =
            new BitmapImage(new Uri("pack://application:,,,/Assets/ui_cone_finish.png"));
    }
}
