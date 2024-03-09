using AxBuilder.HelperProperties;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Newtonsoft.Json.Linq;

namespace AxBuilder
{
    public partial class MainPage : ModernWpf.Controls.Page
    {

        private Viewbox CreateViewbox(MainPage page, Image icon, double width, double height)
        {
            var newViewbox = new Viewbox { Child = icon, Width = width, Height = height };
            newViewbox.MouseDown += Viewbox_MouseDown;
            newViewbox.MouseMove += Viewbox_MouseMove;
            newViewbox.MouseUp += Viewbox_MouseUp;
            newViewbox.MouseWheel += Viewbox_PointerWheelChanged;

            return newViewbox;
        }


        private static Image CreateImageFromIcon(BitmapImage image, double width, double height)
        {
            return new Image { Source = image, Width = width, Height = height };
        }

        private void LoadJson(string json)
        {
            try
            {
                JObject importData = JObject.Parse(json);
                var extraData = (JObject)importData["export_information"];
                var startLine = (JObject)importData["start_line"];
                var finishLine = (JObject)importData["finish_line"];
                var distance = extraData["measured_distance"].Value<double>();
                var scale = extraData["measured_scale"].Value<double>();
                var isImperial = extraData["is_imperial_units"].Value<bool>();
                var imageLocation = extraData["image_file_path"].Value<string>();
                var savedPoints = (JArray)importData["point_data"];

                Units.Tag = isImperial ? 1 : 2;
                if (isImperial)
                {
                    distance *= 3.2808;
                }
                Distance.Text = Math.Round(distance, 3).ToString();
                LoadImage(imageLocation);
                ScaleLength = scale;
                Pixels.Text = Math.Round(scale, 3).ToString();
                ScaleStatus.IsChecked = true;

                foreach (var point in savedPoints)
                {
                    double x = point["x"].Value<double>();
                    double y = point["y"].Value<double>();
                    double rotation = point["rotation"].Value<double>();
                    int coneId = point["cone_id"].Value<int>();
                    var cone = AddCone(coneId, x, y, rotation);
                    if (coneId == 4) { StartGrid = cone; }
                }

                StartLine = AddLine(
                    startLine["cone_id"].Value<int>(),
                    startLine["x1"].Value<double>(),
                    startLine["y1"].Value<double>(),
                    startLine["x2"].Value<double>(),
                    startLine["y2"].Value<double>(),
                    startLine["angle"].Value<double>());

                FinishLine = AddLine(
                    finishLine["cone_id"].Value<int>(),
                    finishLine["x1"].Value<double>(),
                    finishLine["y1"].Value<double>(),
                    finishLine["x2"].Value<double>(),
                    finishLine["y2"].Value<double>(),
                    finishLine["angle"].Value<double>());
            }
            catch (Exception ex)
            {
                ClearAll();
                MessageBox.Show($"Error occurred while loading file: {ex.Message}", "Load Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            SetIsChanged(true);
        }

        private void LoadImage(string imagePath, bool importNew = false)
        {
            try
            {
                string fileName = System.IO.Path.GetFileName(imagePath);
                string localAppDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                string destinationPath = System.IO.Path.Combine(localAppDataFolder, fileName);

                if (File.Exists(destinationPath) && !importNew)
                {
                    ImageLocation = destinationPath;
                }
                else
                {
                    File.Copy(imagePath, destinationPath, true);
                    ImageLocation = destinationPath;
                }

                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.UriSource = new Uri(ImageLocation);
                bitmapImage.EndInit();
                MyImage.Source = bitmapImage;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unable to open/load image file: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }



        private Viewbox AddCone(int coneId, double x, double y, double angle = 0d)
        {
            Image newImage;
            double width;
            double height;
            SetIsChanged();

            if (coneId == 2 || coneId == 4)
            {
                width = 50;
                height = 50;
                newImage = CreateImageFromIcon(GetConeById(coneId), width, height);
                var viewBox = CreateViewbox(this, newImage, width, height);
                viewBox.RenderTransformOrigin = new Point(0.75, 0.5);

                if (angle != 0)
                {
                    viewBox.RenderTransform = new RotateTransform
                    {
                        Angle = angle
                    };
                }

                Canvas.SetLeft(viewBox, x - (width / 4) * 3);
                Canvas.SetTop(viewBox, y - (height / 2));

                MyCanvas.Children.Add(viewBox);
                return viewBox;
            }
            else
            {
                width = 25;
                height = 25;
                newImage = CreateImageFromIcon(GetConeById(coneId), width, height);
                var viewBox = CreateViewbox(this, newImage, width, height);
                viewBox.RenderTransformOrigin = new Point(0.5, 0.5);

                if (angle != 0)
                {
                    viewBox.RenderTransform = new RotateTransform
                    {
                        Angle = angle
                    };
                }

                Canvas.SetLeft(viewBox, x - (width / 2));
                Canvas.SetTop(viewBox, y - (height / 2));

                MyCanvas.Children.Add(viewBox);
                return viewBox;
            }
        }

        private Viewbox AddLine(int coneId, double x1, double y1, double x2, double y2, double angle = 0d)
        {
            var width = 25;
            var height = 25;

            var leftCone = CreateImageFromIcon(GetConeById(coneId), width, height);
            var rightCone = CreateImageFromIcon(GetConeById(coneId), width, height);

            leftCone.RenderTransformOrigin = new Point(0.5, 0.5);
            rightCone.RenderTransformOrigin = new Point(0.5, 0.5);


            leftCone.RenderTransform = new RotateTransform
            {
                Angle = angle
            };
            rightCone.RenderTransform = new RotateTransform
            {
                Angle = angle
            };

            var line = new Line
            {
                Stroke = new SolidColorBrush(Colors.Blue),
                StrokeThickness = 4,
                X1 = x1,
                Y1 = y1,
                X2 = x2,
                Y2 = y2
            };

            var canvas = new Canvas();
            canvas.Width = width;
            canvas.Height = height;
            Canvas.SetLeft(leftCone, x1 - width / 2);
            Canvas.SetTop(leftCone, y1 - height / 2);
            Canvas.SetLeft(rightCone, x2 - width / 2);
            Canvas.SetTop(rightCone, y2 - height / 2);

            canvas.Children.Add(leftCone);
            canvas.Children.Add(rightCone);
            canvas.Children.Add(line);

            var newViewbox = new Viewbox { Child = canvas, Width = width, Height = height };

            newViewbox.RenderTransformOrigin = new Point(0.5, 0.5);

            MyCanvas.Children.Add(newViewbox);

            return newViewbox;
        }


        private void ClearAll()
        {
            // Disable buttons
            NewButton.IsEnabled = false;
            NewButton.Opacity = 0.5;
            SaveButton.IsEnabled = false;
            SaveButton.Opacity = 0.5;
            BuildButton.IsEnabled = false;
            BuildButton.Opacity = 0.5;

            // Set default tool
            activeButton = UprightButton;
            UprightButton.IsChecked = true;
            PointerButton.IsChecked = false;
            StartLineButton.IsChecked = false;
            FinishLineButton.IsChecked = false;
            StartingGridButton.IsChecked = false;
            WallButton.IsChecked = false;
            PointWallButton.IsChecked = false;
            LyingButton.IsChecked = false;
            MeasureScaleButton.IsChecked = false;

            // clear any data
            ImageLocation = string.Empty;
            currentLine = null;
            isManipulating = false;
            isMiddleButtonPressed = false;
            ScaleLength = 0;
            MyImage.Source = null;
            MyCanvas.Children.Clear();
            ScaleStatus.IsChecked = false;
            Distance.Text = string.Empty;
            PlaceholderHelper.SetPlaceholder(Pixels, $"-- px");
            PlaceholderHelper.SetPlaceholder(Distance, "0");
            StartGrid = null;
            StartLine = null;
            FinishLine = null;

            // Set state
            SetIsChanged();

            GC.Collect();
        }


        private BitmapImage GetConeById(int id)
        {
            switch (id)
            {
                case 1:
                    return StandConeImage;
                case 2:
                    return PointConeImage;
                case 3:
                    return LyingConeImage;
                case 4:
                    return CarImage;
                case 5:
                    return StartConeImage;
                case 6:
                    return FinishConeImage;
                default:
                    throw new InvalidOperationException(nameof(id));
            }
        }

        private int GetIdByCone(BitmapImage coneImage)
        {
            if (coneImage == StandConeImage)
            {
                return 1;
            }
            else if (coneImage == PointConeImage)
            {
                return 2;
            }
            else if (coneImage == LyingConeImage)
            {
                return 3;
            }
            else if (coneImage == CarImage)
            {
                return 4;
            }
            else if (coneImage == StartConeImage)
            {
                return 5;
            }
            else if (coneImage == FinishConeImage)
            {
                return 6;
            }
            else
            {
                return -1;
            }
        }

        private void SetIsChanged(bool IsOpenedOrIsSaved = false)
        {
            if (IsOpenedOrIsSaved)
            {
                IsChanged = false;
                NewButton.IsEnabled = true;
                NewButton.Opacity = 1;
                SaveButton.IsEnabled = true;
                SaveButton.Opacity = 1;
                BuildButton.IsEnabled = true;
                BuildButton.Opacity = 1;
            }
            else if (!string.IsNullOrEmpty(ImageLocation) || MyCanvas.Children.Count > 0)
            {
                IsChanged = true;
                NewButton.IsEnabled = true;
                NewButton.Opacity = 1;
                SaveButton.IsEnabled = true;
                SaveButton.Opacity = 1;
                BuildButton.IsEnabled = true;
                BuildButton.Opacity = 1;
            }
            else
            {
                IsChanged = false;
            }
        }
    }
}
