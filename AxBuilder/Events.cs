using System;
using ModernWpf.Controls;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows;
using System.Windows.Shapes;
using System.Windows.Media;
using Microsoft.Win32;
using System.IO;

namespace AxBuilder
{
    public partial class MainPage : ModernWpf.Controls.Page
    {
        private async void NewButton_Click(object sender, RoutedEventArgs e)
        {
            if (await SaveDialog())
            {
                ResetApplication();
            };
        }


        private void OpenImageButton_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image files (*.jpg;*.jpeg;*.png;*.bmp)|*.jpg;*.jpeg;*.png;*.bmp";
            openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);

            if (openFileDialog.ShowDialog() == true)
            {
                string filePath = openFileDialog.FileName;
                var image = LoadImage(filePath, importNew: true);
                if (image is not null)
                {
                    MyImage.Source = image;
                    ImageLocation = filePath;
                }

            }
        }


        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (await SavePreChecks())
            {
                await SaveFile(SaveJsonBuilder());
            }
        }


        private async void OpenButton_Click(object sender, RoutedEventArgs e)
        {
            if (!await SaveDialog())
            {
                return;
            }

            var openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "JSON files (*.json)|*.json";
            openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            if (openFileDialog.ShowDialog() == true)
            {
                string filePath = openFileDialog.FileName;
                string json = File.ReadAllText(filePath);
                LoadJson(json);
                ChangeMainWindowTitleAndText(System.IO.Path.GetFileNameWithoutExtension(openFileDialog.FileName));
            }
        }


        private async void BuildTrack_Click(object sender, RoutedEventArgs e)
        {
            if (await SavePreChecks())
            {
                await BuildTrack();
            }
        }


        private void AppBarToggleButton_Click(object sender, RoutedEventArgs e)
        {
            AppBarToggleButton clickedButton = (AppBarToggleButton)sender;
            if (this.ActiveButton is not null) { this.ActiveButton.IsChecked = false; }
            clickedButton.IsChecked = true;
            this.ActiveButton = clickedButton;

            if (this.ActiveButton == this.MeasureScaleButton)
            {
                this.Distance.IsEnabled = true;
                this.Units.IsEnabled = true;
            }
            else
            {
                this.Distance.IsEnabled = false;
                this.Units.IsEnabled = false;
            }
        }

        // ----------------------------------------------------------------------

        private void MyCanvas_PointerPressed(object sender, MouseButtonEventArgs e)
        {
            if (Mouse.Captured != null || (e.ChangedButton != MouseButton.Left))
            {
                return;
            }


            Mouse.Capture((Canvas)sender);
            var ptrPt = e.GetPosition(this.MyCanvas);
            Viewbox viewbox = null;


            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (this.ActiveButton == this.UprightButton)
                {
                    viewbox = CreateCone(1, ptrPt.X, ptrPt.Y);
                }
                else if (this.ActiveButton == this.PointerButton)
                {
                    viewbox = CreateCone(2, ptrPt.X, ptrPt.Y);
                }
                else if (this.ActiveButton == this.LyingButton)
                {
                    viewbox = CreateCone(3, ptrPt.X, ptrPt.Y);
                }
                else if (this.ActiveButton == this.StartingGridButton)
                {
                    viewbox = CreateCone(4, ptrPt.X, ptrPt.Y);
                    if (StartGrid is not null) { MyCanvas.Children.Remove(StartGrid); }
                    StartGrid = viewbox;
                }
                else
                {
                    this.CurrentLine = new Line
                    {
                        Stroke = new SolidColorBrush(Colors.DarkOrange),
                        StrokeThickness = 2,
                        X1 = e.GetPosition(this.MyCanvas).X,
                        Y1 = e.GetPosition(this.MyCanvas).Y,
                        X2 = e.GetPosition(this.MyCanvas).X,
                        Y2 = e.GetPosition(this.MyCanvas).Y
                    };
                    MyCanvas.Children.Add(this.CurrentLine);
                }
            }

            if (viewbox is not null)
            {
                MyCanvas.Children.Add(viewbox);
                IsChanged = CheckIfChanges();
            }
        }


        private void MyCanvas_PointerMoved(object sender, MouseEventArgs e)
        {
            if (this.CurrentLine is null)
            {
                return;
            }

            var ptrPt = e.GetPosition(this.MyCanvas);

            if (e.LeftButton == MouseButtonState.Pressed)
            {
                double dx = ptrPt.X - CurrentLine.X1;
                double dy = ptrPt.Y - CurrentLine.Y1;

                double angle = Math.Atan2(dy, dx) * 180.0 / Math.PI;

                if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
                {
                    angle = Math.Round(angle / 15.0) * 15.0;
                }

                double distance = Math.Sqrt(dx * dx + dy * dy);
                this.CurrentLine.X2 = this.CurrentLine.X1 + distance * Math.Cos(angle * Math.PI / 180.0);
                this.CurrentLine.Y2 = this.CurrentLine.Y1 + distance * Math.Sin(angle * Math.PI / 180.0);
            }
        }


        private void MyCanvas_PointerReleased(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton != MouseButton.Left)
            {
                return;
            }

            Viewbox viewbox = null;

            if (this.CurrentLine != null)
            {
                double dx = this.CurrentLine.X2 - this.CurrentLine.X1;
                double dy = this.CurrentLine.Y2 - this.CurrentLine.Y1;
                double length = Math.Sqrt(dx * dx + dy * dy);
                double angle = Math.Atan2(dy, dx) * 180.0 / Math.PI;

                if (this.ActiveButton == this.MeasureScaleButton)
                {
                    Pixels.Text = Math.Round(length, 3).ToString();
                }
                else if (this.ActiveButton == this.StartLineButton)
                {
                    viewbox = AddLine(5, this.CurrentLine.X1, this.CurrentLine.Y1, this.CurrentLine.X2, this.CurrentLine.Y2, angle - 90);
                }
                else if (this.ActiveButton == this.FinishLineButton)
                {
                    viewbox = AddLine(6, this.CurrentLine.X1, this.CurrentLine.Y1, this.CurrentLine.X2, this.CurrentLine.Y2, angle - 90);
                }
                else
                {
                    var useCone = this.ActiveButton == this.WallButton ? 1 : 3;

                    double iconSpacing = 50;
                    if (this.ScaleStatus.IsChecked ?? false)
                    {
                        var distance = this.Units.SelectedIndex == 0 ? double.Parse(this.Distance.Text) * 0.3048 : double.Parse(this.Distance.Text);
                        iconSpacing = 4 * (ParsedPixels / distance);
                    }

                    for (double i = 0; i < length + 1; i += iconSpacing)
                    {
                        double t = (i == 0)
                            ? 0
                            : length / i;
                        double x = (i == 0)
                            ? CurrentLine.X1
                            : CurrentLine.X1 + dx / t;
                        double y = (i == 0)
                            ? CurrentLine.Y1
                            : CurrentLine.Y1 + dy / t;

                        MyCanvas.Children.Add(CreateCone(useCone, x, y, angle));
                    }
                    IsChanged = CheckIfChanges();
                }

                this.MyCanvas.Children.Remove(this.CurrentLine);
                this.CurrentLine = null;
            }


            if (viewbox is not null)
            {
                MyCanvas.Children.Add(viewbox);
                IsChanged = CheckIfChanges();
            }

            ((Canvas)sender).ReleaseMouseCapture();
        }

        // ----------------------------------------------------------------------

        internal void Viewbox_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (Mouse.Captured != null || (e.ChangedButton != MouseButton.Right && e.ChangedButton != MouseButton.Left))
            {
                return;
            }

            if (e.RightButton == MouseButtonState.Pressed && e.LeftButton != MouseButtonState.Pressed)
            {
                MyCanvas.Children.Remove((Viewbox)sender);
                return;
            }

            this.IsManipulating = true;
            this.OriginalPointerPosition = e.GetPosition((Viewbox)sender);
            Mouse.Capture((Viewbox)sender);
        }


        internal void Viewbox_MouseMove(object sender, MouseEventArgs e)
        {
            if (this.IsManipulating)
            {
                if (e.LeftButton == MouseButtonState.Pressed)
                {
                    Point currentPosition = e.GetPosition(this.MyCanvas);

                    Canvas.SetLeft((UIElement)sender, currentPosition.X - this.OriginalPointerPosition.X);
                    Canvas.SetTop((UIElement)sender, currentPosition.Y - this.OriginalPointerPosition.Y);
                }
            }
        }


        internal void Viewbox_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton != MouseButton.Left)
            {
                return;
            }
            this.IsManipulating = false;
            this.IsMiddleButtonPressed = false;
            ((Viewbox)sender).ReleaseMouseCapture();
        }


        internal void Viewbox_PointerWheelChanged(object sender, MouseWheelEventArgs e)
        {
            if (this.IsManipulating)
            {
                var viewBox = (Viewbox)sender;
                var rotateTransform = viewBox.RenderTransform as RotateTransform;

                if (rotateTransform == null)
                {
                    rotateTransform = new RotateTransform();
                    viewBox.RenderTransform = rotateTransform;
                }

                var rate = Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift) ? 15 : 5;

                var mouseWheelDelta = e.Delta / 120 * rate;
                rotateTransform.Angle += mouseWheelDelta;
            }
            e.Handled = true;
        }

        // ----------------------------------------------------------------------

        private void Distance_TextChanged(object sender, TextChangedEventArgs e)
        {
            ScaleStatus.IsChecked = false;
            if (e.Source == this.Pixels && !string.IsNullOrEmpty(this.Pixels.Text))
            {
                if (double.TryParse(this.Pixels.Text, out var pixels))
                {
                    ParsedPixels = pixels;
                }
            }
            else if (e.Source == this.Distance && !string.IsNullOrEmpty(this.Distance.Text))
            {
                // remove invalid characters
                if (!double.TryParse(this.Distance.Text, out var dist))
                {
                    this.Distance.Text = this.Distance.Text.Remove(this.Distance.Text.Length - 1, 1);
                    this.Distance.CaretIndex = this.Distance.Text.Length;
                    dist = double.Parse(this.Distance.Text);
                }

                if (dist > 5280 && Units.SelectedIndex == 0)
                {
                    this.Distance.Text = "5280";
                    ParsedDistance = 5280;
                }
                else if (dist > 1609 && Units.SelectedIndex == 1)
                {
                    this.Distance.Text = "1609";
                    ParsedDistance = 1609;
                }
                else
                {
                    ParsedDistance = dist;
                }
            }

            if (ParsedPixels != 0 && ParsedDistance != 0)
            {
                ScaleStatus.IsChecked = true;
            }
        }


        private void Units_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            IsImperial = (Units.SelectedIndex == 0);
            // trigger change in distance box
            if (!string.IsNullOrEmpty(this.Distance.Text))
            {
                Distance.Text = Distance.Text;
            }
        }

        // ----------------------------------------------------------------------

        private void MyScrollViewer_PointerPressed(object sender, MouseButtonEventArgs e)
        {
            if (Mouse.Captured != null)
            {
                return;
            }
            if (e.ChangedButton == MouseButton.Middle)
            {
                this.IsMiddleButtonPressed = true;
                this.OriginalPointerPosition = e.GetPosition(MyGrid);
                this.MyGrid.CaptureMouse();
                this.MyImage.RenderTransform = this.Transform;
                this.MyCanvas.RenderTransform = this.Transform;
            }
        }


        private void MyScrollViewer_PointerMoved(object sender, MouseEventArgs e)
        {
            if (IsMiddleButtonPressed && Mouse.Captured == MyGrid)
            {
                var currentPointerPosition = e.GetPosition(MyGrid);
                Transform.X += currentPointerPosition.X - OriginalPointerPosition.X;
                Transform.Y += currentPointerPosition.Y - OriginalPointerPosition.Y;
                OriginalPointerPosition = currentPointerPosition;
            }
        }


        private void MyScrollViewer_PointerReleased(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton != MouseButton.Middle)
            {
                return;
            }
            this.MyGrid.ReleaseMouseCapture();
            this.IsMiddleButtonPressed = false;
        }


        private void MyScrollViewer_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (Mouse.Captured != null)
            {
                return;
            }
            ScaleTransform scaleTransform = this.MyImage.LayoutTransform as ScaleTransform;

            if (scaleTransform == null)
            {
                scaleTransform = new ScaleTransform(1, 1);
                this.MyCanvas.LayoutTransform = scaleTransform;
                this.MyImage.LayoutTransform = scaleTransform;
            }

            double minZoom = 0.1;
            double maxZoom = 3.0;

            // Get the center point of the ScrollViewer
            Point scrollViewerCenter = new Point(this.MyScrollViewer.ActualWidth / 2, this.MyScrollViewer.ActualHeight / 2);

            double zoomFactor = e.Delta > 0 ? 1.1 : 0.9; // Increase or decrease zoom factor

            // Calculate new scale factor
            double newScaleX = scaleTransform.ScaleX * zoomFactor;
            double newScaleY = scaleTransform.ScaleY * zoomFactor;

            double clampedScaleX = Math.Max(minZoom, Math.Min(maxZoom, newScaleX));
            double clampedScaleY = Math.Max(minZoom, Math.Min(maxZoom, newScaleY));

            // Adjust the zoom factor
            zoomFactor = clampedScaleX / scaleTransform.ScaleX;

            // Apply the scale
            scaleTransform.ScaleX = clampedScaleX;
            scaleTransform.ScaleY = clampedScaleY;

            // Calculate the new offset based on the center of the ScrollViewer
            double offsetX = (scrollViewerCenter.X - e.GetPosition(this.MyScrollViewer).X) * (zoomFactor - 1);
            double offsetY = (scrollViewerCenter.Y - e.GetPosition(this.MyScrollViewer).Y) * (zoomFactor - 1);

            // Adjust the translation to center the zoom around the center of the ScrollViewer
            TranslateTransform translateTransform = this.MyCanvas.RenderTransform as TranslateTransform;
            if (translateTransform == null)
            {
                translateTransform = new TranslateTransform();
                MyCanvas.RenderTransform = translateTransform;
                MyImage.LayoutTransform = translateTransform;
            }

            // Adjust canvas translation
            translateTransform.X -= offsetX;
            translateTransform.Y -= offsetY;

            e.Handled = true; // Prevent scrolling the ScrollViewer's content
        }

        // ----------------------------------------------------------------------

    }
}
