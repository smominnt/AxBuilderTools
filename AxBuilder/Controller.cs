using System;
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
                ResetApplication();
                JObject importData = JObject.Parse(json);
                var extraData = (JObject)importData["export_information"];
                var startLine = (JObject)importData["start_line"];
                var finishLine = (JObject)importData["finish_line"];
                var distance = extraData["measured_distance"].Value<double>();
                var scale = extraData["measured_scale"].Value<double>();
                var isImperial = extraData["is_imperial_units"].Value<bool>();
                var imageLocation = extraData["image_file_path"].Value<string>();
                var savedPoints = (JArray)importData["point_data"];

                this.Units.Tag = isImperial ? 1 : 2;
                this.Distance.Text = Math.Round(distance, 3).ToString();
                this.Pixels.Text = Math.Round(scale, 3).ToString();

                var image = this.LoadImage(imageLocation);
                if (image is not null)
                {
                    MyImage.Source = image;
                    ImageLocation = imageLocation;
                }


                foreach (var point in savedPoints)
                {
                    double x = point["x"].Value<double>();
                    double y = point["y"].Value<double>();
                    double rotation = point["rotation"].Value<double>();
                    int coneId = point["cone_id"].Value<int>();
                    var cone = CreateCone(coneId, x, y, rotation);
                    MyCanvas.Children.Add(cone);
                    if (coneId == 4) 
                    {
                        if (StartGrid is not null) { MyCanvas.Children.Remove(StartGrid); }
                        StartGrid = cone; 
                    }
                }

                StartLine = AddLine(
                    startLine["cone_id"].Value<int>(),
                    startLine["x1"].Value<double>(),
                    startLine["y1"].Value<double>(),
                    startLine["x2"].Value<double>(),
                    startLine["y2"].Value<double>(),
                    startLine["angle"].Value<double>());
                MyCanvas.Children.Add(StartLine);

                FinishLine = AddLine(
                    finishLine["cone_id"].Value<int>(),
                    finishLine["x1"].Value<double>(),
                    finishLine["y1"].Value<double>(),
                    finishLine["x2"].Value<double>(),
                    finishLine["y2"].Value<double>(),
                    finishLine["angle"].Value<double>());
                MyCanvas.Children.Add(FinishLine);
                IsChanged = CheckIfChanges(true);
            }
            catch (Exception ex)
            {
                ResetApplication();
                MessageBox.Show($"Error occurred while loading file: {ex.Message}", "Load Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private BitmapImage LoadImage(string imagePath, bool importNew = false)
        {
            try
            {
                string fileName = System.IO.Path.GetFileName(imagePath);
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.UriSource = new Uri(imagePath);
                bitmapImage.EndInit();

                if (importNew)
                {
                    IsChanged = CheckIfChanges();
                }
                return bitmapImage;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unable to open/load image file: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }


        private Viewbox CreateCone(int coneId, double x, double y, double angle = 0d)
        {
            Image newImage;
            double width;
            double height;
            Viewbox viewbox;

            if (coneId == 2 || coneId == 4)
            {
                width = 50;
                height = 50;
                newImage = CreateImageFromIcon(GetConeById(coneId), width, height);
                viewbox = CreateViewbox(this, newImage, width, height);
                viewbox.RenderTransformOrigin = new Point(0.75, 0.5);

                if (angle != 0)
                {
                    viewbox.RenderTransform = new RotateTransform
                    {
                        Angle = angle
                    };
                }

                Canvas.SetLeft(viewbox, x - (width / 4) * 3);
                Canvas.SetTop(viewbox, y - (height / 2));
            }
            else
            {
                width = 25;
                height = 25;
                newImage = CreateImageFromIcon(GetConeById(coneId), width, height);
                viewbox = CreateViewbox(this, newImage, width, height);
                viewbox.RenderTransformOrigin = new Point(0.5, 0.5);

                if (angle != 0)
                {
                    viewbox.RenderTransform = new RotateTransform
                    {
                        Angle = angle
                    };
                }

                Canvas.SetLeft(viewbox, x - (width / 2));
                Canvas.SetTop(viewbox, y - (height / 2));
            }

            return viewbox;
        }


        private Viewbox AddLine(int coneId, double x1, double y1, double x2, double y2, double angle = 0d)
        {
            var width = 25;
            var height = 25;

            var coneType = GetConeById(coneId);
            var leftCone = CreateImageFromIcon(coneType, width, height);
            var rightCone = CreateImageFromIcon(coneType, width, height);

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


            if (coneType == StartConeImage)
            {
                if (StartLine is not null) { MyCanvas.Children.Remove(StartLine); }
                StartLine = newViewbox;
            }
            else if (coneType == FinishConeImage)
            {
                if (FinishLine is not null) { MyCanvas.Children.Remove(FinishLine); }
                FinishLine = newViewbox;
            }

            return newViewbox;
        }


        private static BitmapImage GetConeById(int id)
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


        private static int GetIdByCone(BitmapImage coneImage)
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


        private bool CheckIfChanges(bool IsOpenedOrIsSaved = false)
        {
            if (IsOpenedOrIsSaved || !string.IsNullOrEmpty(ImageLocation) || MyCanvas.Children.Count > 0)
            {
                NewButton.IsEnabled = true;
                NewButton.Opacity = 1;
                SaveButton.IsEnabled = true;
                SaveButton.Opacity = 1;
                BuildButton.IsEnabled = true;
                BuildButton.Opacity = 1;
                return !IsOpenedOrIsSaved;
            }
            else
            {
                return false;
            }
        }
    }
}
