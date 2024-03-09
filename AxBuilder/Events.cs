using System;
using System.Linq;
using ModernWpf.Controls;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows;
using System.Windows.Shapes;
using System.Windows.Media;
using Microsoft.Win32;
using System.IO;
using System.Runtime.InteropServices;

namespace AxBuilder
{
    public partial class MainPage : ModernWpf.Controls.Page
    {
        private async void NewButton_Click(object sender, RoutedEventArgs e)
        {
            if (await SaveDialog())
            {
                ClearAll();
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
                LoadImage(filePath, importNew: true);
            }
        }


        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (await SavePreChecks())
            {
                if (await SaveFile(SaveJsonBuilder()))
                {
                    SetIsChanged(true);
                }
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
            }
        }


        private async void BuildTrack_Click(object sender, RoutedEventArgs e)
        {
            if (await SavePreChecks())
            {
                var saveJson = SaveJsonBuilder();
                if (IsChanged) 
                {
                    MessageBoxResult result = await ShowMessageBoxAsync("Do you want to save the map file?", "Save Map file", true);
                    if (result == MessageBoxResult.Yes)
                    {
                        await SaveFile(saveJson);
                    }
                }
                await BuildTrack(saveJson);
            }
        }


        private void AppBarToggleButton_Click(object sender, RoutedEventArgs e)
        {
            // Deactivate the previously active button
            if (activeButton != null)
            {
                activeButton.IsChecked = false;
            }

            // Activate the clicked button and update the active button
            AppBarToggleButton clickedButton = (AppBarToggleButton)sender;
            clickedButton.IsChecked = true;
            activeButton = clickedButton;

            if (activeButton == MeasureScaleButton)
            {
                Distance.IsEnabled = true;
                Units.IsEnabled = true;
            }
            else
            {
                Distance.IsEnabled = false;
                Units.IsEnabled = false;
            }
        }


        private void MyCanvas_PointerPressed(object sender, MouseButtonEventArgs e)
        {
            if (Mouse.Captured != null || (e.ChangedButton != MouseButton.Left) || activeButton == null)
            {
                return;
            }


            Mouse.Capture((Canvas)sender);
            var ptrPt = e.GetPosition(MyCanvas);


            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (activeButton == UprightButton)
                {
                    AddCone(1, ptrPt.X, ptrPt.Y);
                }
                else if (activeButton == PointerButton)
                {
                    AddCone(2, ptrPt.X, ptrPt.Y);
                }
                else if (activeButton == LyingButton)
                {
                    AddCone(3, ptrPt.X, ptrPt.Y);
                }
                else if (activeButton == StartingGridButton)
                {
                    if (StartGrid != null)
                    {
                        MyCanvas.Children.Remove(StartGrid);
                    }
                    StartGrid = AddCone(4, ptrPt.X, ptrPt.Y);
                }
                else
                {
                    currentLine = new Line
                    {
                        Stroke = new SolidColorBrush(Colors.DarkOrange),
                        StrokeThickness = 2,
                        X1 = e.GetPosition(MyCanvas).X,
                        Y1 = e.GetPosition(MyCanvas).Y,
                        X2 = e.GetPosition(MyCanvas).X,
                        Y2 = e.GetPosition(MyCanvas).Y
                    };
                    MyCanvas.Children.Add(currentLine);
                }
            }
        }


        private void MyCanvas_PointerMoved(object sender, MouseEventArgs e)
        {
            if (currentLine == null)
            {
                return;
            }

            var ptrPt = e.GetPosition(MyCanvas);

            if (e.LeftButton == MouseButtonState.Pressed)
            {
                double dx = ptrPt.X - currentLine.X1;
                double dy = ptrPt.Y - currentLine.Y1;

                double angle = Math.Atan2(dy, dx) * 180.0 / Math.PI;

                if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
                {
                    angle = Math.Round(angle / 15.0) * 15.0;
                }

                double distance = Math.Sqrt(dx * dx + dy * dy);
                currentLine.X2 = currentLine.X1 + distance * Math.Cos(angle * Math.PI / 180.0);
                currentLine.Y2 = currentLine.Y1 + distance * Math.Sin(angle * Math.PI / 180.0);
            }
        }


        private void MyCanvas_PointerReleased(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton != MouseButton.Left)
            {
                return;
            }

            if (currentLine != null)
            {
                double dx = currentLine.X2 - currentLine.X1;
                double dy = currentLine.Y2 - currentLine.Y1;
                double length = Math.Sqrt(dx * dx + dy * dy);
                double angle = Math.Atan2(dy, dx) * 180.0 / Math.PI;

                if (activeButton == MeasureScaleButton)
                {
                    ScaleLength = length;
                    Pixels.Text = $"{Math.Round(length, 3)} px";
                }
                else if (activeButton == StartLineButton)
                {
                    if (StartLine != null)
                    {
                        MyCanvas.Children.Remove(StartLine);
                    }
                    StartLine = AddLine(5, currentLine.X1, currentLine.Y1, currentLine.X2, currentLine.Y2, angle - 90);
                }
                else if (activeButton == FinishLineButton)
                {
                    if (FinishLine != null)
                    {
                        MyCanvas.Children.Remove(FinishLine);
                    }
                    FinishLine = AddLine(6, currentLine.X1, currentLine.Y1, currentLine.X2, currentLine.Y2, angle - 90);
                }
                else
                {
                    var useCone = activeButton == WallButton ? 1 : 3;

                    double iconSpacing = 50;
                    if (ScaleStatus.IsChecked ?? false)
                    {
                        var distance = Units.SelectedIndex == 0 ? double.Parse(Distance.Text) * 0.3048 : double.Parse(Distance.Text);
                        iconSpacing = 4 * (ScaleLength / distance);
                    }

                    for (double i = 0; i < length + 1; i += iconSpacing)
                    {
                        double t = (i == 0)
                            ? 0
                            : length / i;
                        double x = (i == 0)
                            ? currentLine.X1
                            : currentLine.X1 + dx / t;
                        double y = (i == 0)
                            ? currentLine.Y1
                            : currentLine.Y1 + dy / t;

                        AddCone(useCone, x, y, angle);
                    }
                }

                MyCanvas.Children.Remove(currentLine);
                currentLine = null;
            }

            ((Canvas)sender).ReleaseMouseCapture();
        }

        private void Viewbox_MouseDown(object sender, MouseButtonEventArgs e)
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

            isManipulating = true;
            originalPointerPosition = e.GetPosition((Viewbox)sender);
            Mouse.Capture((Viewbox)sender);
        }

        private void Viewbox_MouseMove(object sender, MouseEventArgs e)
        {
            if (isManipulating)
            {
                if (e.LeftButton == MouseButtonState.Pressed)
                {
                    Point currentPosition = e.GetPosition(MyCanvas);

                    Canvas.SetLeft((UIElement)sender, currentPosition.X - originalPointerPosition.X);
                    Canvas.SetTop((UIElement)sender, currentPosition.Y - originalPointerPosition.Y);
                }
            }
        }

        private void Viewbox_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton != MouseButton.Left)
            {
                return;
            }
            isManipulating = false;
            isMiddleButtonPressed = false;
            ((Viewbox)sender).ReleaseMouseCapture();
        }



        private void Viewbox_PointerWheelChanged(object sender, MouseWheelEventArgs e)
        {
            if (isManipulating)
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


        private void Distance_TextChanged(object sender, TextChangedEventArgs e)
        {
            ScaleStatus.IsChecked = false;
            if (!string.IsNullOrEmpty(Distance.Text))
            {
                // remove invalid characters
                if (!double.TryParse(Distance.Text, out var dist))
                {
                    Distance.Text = Distance.Text.Remove(Distance.Text.Length - 1, 1);
                    Distance.CaretIndex = Distance.Text.Length;
                }

                if (dist > 5280 && Units.SelectedIndex == 0)
                {
                    Distance.Text = "5280";
                }
                else if (dist > 1609 && Units.SelectedIndex == 1)
                {
                    Distance.Text = "1609";
                }

                if (dist > 0 && ScaleLength > 0)
                {
                    ScaleStatus.IsChecked = true;
                }
            }
        }

        private void Units_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!double.TryParse(Distance.Text, out var dist))
            {
                return;
            }
            else if (dist > 5280 && Units.SelectedIndex == 0)
            {
                Distance.Text = "5280";
            }
            else if (dist > 1609 && Units.SelectedIndex == 1)
            {
                Distance.Text = "1609";
            }
        }


        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = e.Text.Any(c => c != '.' && !char.IsDigit(c));
        }


        private void MyScrollViewer_PointerPressed(object sender, MouseButtonEventArgs e)
        {
            if (Mouse.Captured != null)
            {
                return;
            }
            if (e.ChangedButton == MouseButton.Middle)
            {
                isMiddleButtonPressed = true;
                originalPointerPosition = e.GetPosition(MyGrid);
                MyGrid.CaptureMouse();
                MyImage.RenderTransform = transform;
                MyCanvas.RenderTransform = transform;
            }
        }


        private void MyScrollViewer_PointerMoved(object sender, MouseEventArgs e)
        {
            if (isMiddleButtonPressed && Mouse.Captured == MyGrid)
            {
                var currentPointerPosition = e.GetPosition(MyGrid);
                transform.X += currentPointerPosition.X - originalPointerPosition.X;
                transform.Y += currentPointerPosition.Y - originalPointerPosition.Y;
                originalPointerPosition = currentPointerPosition;
            }
        }


        private void MyScrollViewer_PointerReleased(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton != MouseButton.Middle)
            {
                return;
            }
            MyGrid.ReleaseMouseCapture();
            isMiddleButtonPressed = false;
        }


        private void MyScrollViewer_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (Mouse.Captured != null)
            {
                return;
            }
            ScaleTransform scaleTransform = MyImage.LayoutTransform as ScaleTransform;

            if (scaleTransform == null)
            {
                scaleTransform = new ScaleTransform(1, 1);
                MyCanvas.LayoutTransform = scaleTransform;
                MyImage.LayoutTransform = scaleTransform;
            }

            double minZoom = 0.1;
            double maxZoom = 3.0;

            // Get the center point of the ScrollViewer
            Point scrollViewerCenter = new Point(MyScrollViewer.ActualWidth / 2, MyScrollViewer.ActualHeight / 2);

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
            double offsetX = (scrollViewerCenter.X - e.GetPosition(MyScrollViewer).X) * (zoomFactor - 1);
            double offsetY = (scrollViewerCenter.Y - e.GetPosition(MyScrollViewer).Y) * (zoomFactor - 1);

            // Adjust the translation to center the zoom around the center of the ScrollViewer
            TranslateTransform translateTransform = MyCanvas.RenderTransform as TranslateTransform;
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

    }
}
