using System;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows;

namespace AxBuilder
{
    public partial class MainPage : ModernWpf.Controls.Page
    {
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

        public string ImageLocation;

        private ModernWpf.Controls.AppBarToggleButton activeButton;

        private Line currentLine;

        private Point originalPointerPosition;

        private double ZoomFactor = 1;

        private TranslateTransform transform = new TranslateTransform();

        private bool isManipulating = false;

        private bool isMiddleButtonPressed = false;

        private bool isActionActive = false;

        private double ScaleLength = 0;

        private bool IsChanged = false;

        private Viewbox StartGrid = null;

        private Viewbox StartLine = null;

        private Viewbox FinishLine = null;
    }
}
