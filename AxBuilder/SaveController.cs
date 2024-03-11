using Microsoft.Win32;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Newtonsoft.Json.Linq;
using System.Runtime.InteropServices;
using System.Diagnostics;
using Path = System.IO.Path;
using System.Text;

namespace AxBuilder
{
    public partial class MainPage : ModernWpf.Controls.Page
    {
        private string SaveJsonBuilder()
        {
            JObject exportData = new JObject();
            JObject extraData = new JObject();
            JArray savedPoints = new JArray();
            JObject startLine = new JObject();
            JObject finishLine = new JObject();

            var distance = double.Parse(Distance.Text);
            var isImperial = IsImperial;
            double centerX = MyImage.ActualWidth / 2;
            double centerY = MyImage.ActualHeight / 2;
            double fbxRatio = distance / ParsedPixels;
            if (isImperial)
            {
                fbxRatio *= 0.3048;
            }

            // Add export information
            extraData["measured_distance"] = distance;
            extraData["measured_scale"] = ParsedPixels;
            extraData["is_imperial_units"] = isImperial;
            extraData["fbx_ratio"] = fbxRatio;
            extraData["image_file_path"] = ImageLocation ?? string.Empty;

            exportData["export_information"] = extraData;

            var cones = MyCanvas.Children.OfType<Viewbox>();
            foreach (var cone in cones)
            {
                if (cone == StartLine)
                {
                    var line = (cone.Child as Canvas).Children.OfType<Line>().First();
                    var lineCone = (cone.Child as Canvas).Children.OfType<Image>().First();
                    var rTransform = lineCone.RenderTransform as RotateTransform;
                    double angle = rTransform != null ? rTransform.Angle : 0d;
                    startLine["x1"] = line.X1;
                    startLine["y1"] = line.Y1;
                    startLine["x2"] = line.X2;
                    startLine["y2"] = line.Y2;
                    startLine["fbx_x1"] = (line.X1 - centerX) * fbxRatio;
                    startLine["fbx_y1"] = (centerY - line.Y1) * fbxRatio;
                    startLine["fbx_x2"] = (line.X2 - centerX) * fbxRatio;
                    startLine["fbx_y2"] = (centerY - line.Y2) * fbxRatio;
                    startLine["angle"] = angle;
                    startLine["cone_id"] = GetIdByCone(StartConeImage);
                }
                else if (cone == FinishLine)
                {
                    var line = (cone.Child as Canvas).Children.OfType<Line>().First();
                    var lineCone = (cone.Child as Canvas).Children.OfType<Image>().First();
                    var rTransform = lineCone.RenderTransform as RotateTransform;
                    double angle = rTransform != null ? rTransform.Angle : 0d;
                    finishLine["x1"] = line.X1;
                    finishLine["y1"] = line.Y1;
                    finishLine["x2"] = line.X2;
                    finishLine["y2"] = line.Y2;
                    finishLine["fbx_x1"] = (line.X1 - centerX) * fbxRatio;
                    finishLine["fbx_y1"] = (centerY - line.Y1) * fbxRatio;
                    finishLine["fbx_x2"] = (line.X2 - centerX) * fbxRatio;
                    finishLine["fbx_y2"] = (centerY - line.Y2) * fbxRatio;
                    finishLine["angle"] = angle;
                    finishLine["cone_id"] = GetIdByCone(FinishConeImage);
                }
                else
                {
                    var coneBitmap = (cone.Child as Image).Source as BitmapImage;
                    int coneId = GetIdByCone(coneBitmap);
                    double x;
                    double y;

                    switch (coneId)
                    {
                        case 2:
                        case 4:
                            x = Canvas.GetLeft(cone) + (cone.Width / 4) * 3;
                            y = Canvas.GetTop(cone) + (cone.Height / 2);
                            break;
                        default:
                            x = Canvas.GetLeft(cone) + (cone.Width / 2);
                            y = Canvas.GetTop(cone) + (cone.Height / 2);
                            break;
                    }

                    var rTransform = cone.RenderTransform as RotateTransform;
                    double angle = rTransform != null ? rTransform.Angle : 0d;

                    JObject pointData = new JObject
                    {
                        ["x"] = x,
                        ["y"] = y,
                        ["fbx_x"] = (x - centerX) * fbxRatio,
                        ["fbx_y"] = (centerY - y) * fbxRatio,
                        ["rotation"] = angle,
                        ["cone_id"] = coneId
                    };

                    savedPoints.Add(pointData);
                }
            }

            exportData["point_data"] = savedPoints;
            exportData["start_line"] = startLine;
            exportData["finish_line"] = finishLine;

            return exportData.ToString();
        }

        public async Task<bool> SaveDialog(bool isBuildingTrack = false)
        {
            var tcs = new TaskCompletionSource<bool>();
            if (IsChanged)
            {
                MessageBoxResult result = MessageBox.Show("Save current file?", "Save", MessageBoxButton.YesNoCancel);

                switch (result)
                {
                    case MessageBoxResult.Yes:
                        if (!await SavePreChecks())
                        {
                            tcs.SetResult(false);
                        }
                        else
                        {
                            var json = SaveJsonBuilder();
                            if (!await SaveFile(json))
                            {
                                tcs.SetResult(false);
                            }
                            else
                            {
                                tcs.SetResult(true);
                            }
                        }
                        break;

                    case MessageBoxResult.No:
                        if (!isBuildingTrack) 
                        {
                            ResetApplication();
                        }
                        tcs.SetResult(true);
                        break;

                    case MessageBoxResult.Cancel:
                        tcs.SetResult(false);
                        break;
                }
            }
            else
            {
                tcs.SetResult(true);
            }
            return await tcs.Task;
        }

        private async Task<bool> SavePreChecks()
        {
            if (!(ScaleStatus.IsChecked ?? true))
            {
                MessageBoxResult result = await ShowMessageBoxAsync("Set map scale before saving.", "Map Scale");
                if (result == MessageBoxResult.OK)
                {
                    this.ActiveButton.IsChecked = false;
                    this.ActiveButton = MeasureScaleButton;
                    this.ActiveButton.IsChecked = true;
                    Distance.IsEnabled = true;
                    Units.IsEnabled = true;
                    return false;
                }
                else
                {
                    return false;
                }
            }
            else if (StartGrid == null)
            {
                MessageBoxResult result = await ShowMessageBoxAsync("Set Start Grid before saving.", "Start Grid");
                if (result == MessageBoxResult.OK)
                {
                    this.ActiveButton.IsChecked = false;
                    this.ActiveButton = StartingGridButton;
                    this.ActiveButton.IsChecked = true;
                    Distance.IsEnabled = false;
                    Units.IsEnabled = false;
                    return false;
                }
                else
                {
                    return false;
                }
            }
            else if (StartLine == null)
            {
                MessageBoxResult result = await ShowMessageBoxAsync("Set Start Line before saving.", "Start Line");
                if (result == MessageBoxResult.OK)
                {
                    this.ActiveButton.IsChecked = false;
                    this.ActiveButton = StartLineButton;
                    this.ActiveButton.IsChecked = true;
                    Distance.IsEnabled = false;
                    Units.IsEnabled = false;
                    return false;
                }
                else
                {
                    return false;
                }
            }
            else if (FinishLine == null)
            {
                MessageBoxResult result = await ShowMessageBoxAsync("Set Finish Line before saving.", "Finish Line");
                if (result == MessageBoxResult.OK)
                {
                    this.ActiveButton.IsChecked = false;
                    this.ActiveButton = FinishLineButton;
                    this.ActiveButton.IsChecked = true;
                    Distance.IsEnabled = false;
                    Units.IsEnabled = false;
                    return false;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return true;
            }
        }

        private Task<MessageBoxResult> ShowMessageBoxAsync(string message, string caption, bool YesNo = false)
        {
            var tcs = new TaskCompletionSource<MessageBoxResult>();
            Application.Current.Dispatcher.Invoke(() =>
            {
                tcs.SetResult(MessageBox.Show(message, caption, YesNo ? MessageBoxButton.YesNo : MessageBoxButton.OK));
            });
            return tcs.Task;
        }

        private async Task<bool> SaveFile(string json)
        {
            var saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "JSON files (*.json)|*.json";
            saveFileDialog.Title = "Save JSON File";

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    string fileName = saveFileDialog.FileName;

                    using (var fileStream = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 4096, useAsync: true))
                    using (var streamWriter = new StreamWriter(fileStream))
                    {
                        await streamWriter.WriteAsync(json);
                        MessageBox.Show($"Map file was saved to: {fileName}", "Saved");
                        IsChanged = CheckIfChanges(true);
                    }

                    ChangeMainWindowTitleAndText(Path.GetFileNameWithoutExtension(fileName));
                    return true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Unable to save map file: {ex.Message}", "Error");
                    return false;
                }
            }
            else
            {
                MessageBox.Show("No file was selected.", "Save Cancelled");
                return false;
            }
        }

        private async Task<bool> BuildTrack()
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

            var saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "KN5 files (*.kn5)|*.kn5";
            try
            {
                if (saveFileDialog.ShowDialog() == true)
                {
                    // Generate temp directory
                    var tempDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
                    Directory.CreateDirectory(tempDirectory);
                    var fbxFile = Path.Combine(tempDirectory, "output.fbx");
                    var kn5File = saveFileDialog.FileName;

                    // build fbx
                    StringBuilder errorMessage = new StringBuilder(256);
                    if (AxFbxBuilderDll
                        .BuildMapFbx(
                            out IntPtr pData, 
                            out int length, 
                            saveJson, 
                            AppDomain.CurrentDomain.BaseDirectory, 
                            errorMessage, 
                            errorMessage.Capacity) != 0)
                    {
                        throw new Exception(errorMessage.ToString());
                    }
                    
                    byte[] data = new byte[length];
                    Marshal.Copy(pData, data, 0, length);
                    await File.WriteAllBytesAsync(fbxFile, data);
                    AxFbxBuilderDll.FreeMemory(pData);

                    // copy textures
                    var inputTextures = Path.Combine(Directory.GetCurrentDirectory(), "texture");
                    var outputTextures = Path.Combine(tempDirectory, "texture");
                    Directory.CreateDirectory(outputTextures);
                    foreach (string file in Directory.GetFiles(inputTextures))
                    {
                        string fileName = Path.GetFileName(file);
                        string destinationPath = Path.Combine(outputTextures, fileName);
                        File.Copy(file, destinationPath, true);
                    }

                    // start fbx to kn5 conversion
                    ProcessStartInfo startInfo = new ProcessStartInfo();
                    startInfo.FileName = "simplekn5converter.exe";
                    startInfo.Arguments = $"kn5track \"{kn5File}\" \"{fbxFile}\"";

                    using (Process process = Process.Start(startInfo))
                    {
                        process.WaitForExit();
                    }

                    // clean up
                    Directory.Delete(tempDirectory, true);
                    return true;
                }
                else
                {
                    MessageBox.Show("No file was selected.", "Build Cancelled", MessageBoxButton.OK, MessageBoxImage.Information);
                    return false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unable to build track file: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

    }
}
