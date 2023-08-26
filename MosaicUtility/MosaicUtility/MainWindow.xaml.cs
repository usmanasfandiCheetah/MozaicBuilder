using Microsoft.Win32;
using MosaicUtility.Classes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using SD = System.Drawing;
using SDI = System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Text.RegularExpressions;
using System.Net;
using Newtonsoft;
using RestSharp;
using System.Net.NetworkInformation;
using System.Diagnostics;

namespace MosaicUtility
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        MosaicHelper mosaic;
        DirectoryWatcher watcher = new DirectoryWatcher();
        DispatcherTimer timer;
        SD.Bitmap bSliceImage = null;
        int sliceWidth = 0;
        int sliceHeight = 0;
        int totalCols = 0;
        int totalRows = 0;
        double screenWidth = 0;
        double screenHeight = 0;
        bool jobSaved = false;
        string PhotoSource = "";
        string[] photos;
        Size wall;
        Size sticker;

        public MainWindow()
        {
            InitializeComponent();

            mosaic = new MosaicHelper(Globals.DataFolder);
            screenWidth = System.Windows.SystemParameters.VirtualScreenWidth;
            screenHeight = System.Windows.SystemParameters.VirtualScreenHeight;

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromTicks(500);
            timer.Tick += Timer_Tick;
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            lblDate.Content = DateTime.Now.ToString("dd/MM/yy HH:mm:ss");
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            timer.Start();

            IDictionary<int, string> BlendModes = Globals.ReadEnum<BlendingMode>();
            cboBlendMode.ItemsSource = BlendModes;
            cboBlendMode.DisplayMemberPath = "Value";
            cboBlendMode.SelectedValuePath = "Key";
            ResetJob();
            LoadConfig();
        }

        void LoadConfig()
        {
            string filename = Globals.DataFolder + "config.dll";
            if (File.Exists(filename))
            {
                string s = File.ReadAllText(filename);
                Job j = Newtonsoft.Json.JsonConvert.DeserializeObject<Job>(s);
                if (j != null)
                {
                    txtBackImage.Text = j.BackImagePath;
                    if (!string.IsNullOrEmpty(txtBackImage.Text))
                    {
                        if (File.Exists(txtBackImage.Text))
                        {
                            BitmapImage img = new BitmapImage(new Uri(txtBackImage.Text));
                            imgFrame.Source = img;
                            lblResolution.Content = img.PixelWidth.ToString() + " x " + img.PixelHeight.ToString();
                        }
                    }
                    txtOverlay.Text = j.OverlayImagePath;
                    if (!string.IsNullOrEmpty(txtOverlay.Text))
                    {
                        if (File.Exists(txtOverlay.Text))
                        {
                            BitmapImage imgO = new BitmapImage(new Uri(txtOverlay.Text));
                            imgOverlay.Source = imgO;
                        }
                    }
                    txtFrameWidth.Text = j.FrameWidth;
                    txtFrameHeight.Text = j.FrameHeight;

                    lblFrameSize.Content = j.FrameWidth + " x " + j.FrameHeight;

                    txtStickerWidth.Text = j.StickerWidth;
                    txtStickerHeight.Text = j.StickerHeight;
                    lblTotalCells.Content = j.TotalStickers;
                    txtBlend.Number = Convert.ToInt32(j.BlendValue);
                    txtBrightness.Number = Convert.ToInt32(j.Brightness);
                    txtContrast.Number = Convert.ToInt32(j.Contrast);
                    chkPrint.IsChecked = j.PrintingEnabled == 1;
                    cboPrinters.Text = j.PrinterName;
                    txtTotalPhotosPerPrint.Text = j.PhotosPerPrint;
                    txtSource.Text = j.DirWatcherPath;
                    txtEventCodes.Text = j.EventCodes;
                    chkImmediatePrint.IsChecked = j.ImmediatePrint == 1;
                }
            }
        }

        void SaveConfig()
        {
            Job j = new Job();
            j.BackImagePath = txtBackImage.Text;
            j.OverlayImagePath = txtOverlay.Text;
            j.FrameWidth = txtFrameWidth.Text;
            j.FrameHeight = txtFrameHeight.Text;
            j.StickerWidth = txtStickerWidth.Text;
            j.StickerHeight = txtStickerHeight.Text;
            j.TotalStickers = lblTotalCells.Content.ToString();
            j.BlendValue = txtBlend.Number.ToString();
            j.Brightness = txtBrightness.Number.ToString();
            j.Contrast = txtContrast.Number.ToString();
            j.PrintingEnabled = (bool)chkPrint.IsChecked ? 1 : 0;
            j.PrinterName = cboPrinters.Text;
            j.PhotosPerPrint = txtTotalPhotosPerPrint.Text;
            j.DirWatcherPath = txtSource.Text;
            j.EventCodes = txtEventCodes.Text;
            j.ImmediatePrint = (bool)chkImmediatePrint.IsChecked ? 1 : 0;

            string s = Newtonsoft.Json.JsonConvert.SerializeObject(j);
            File.WriteAllText(Globals.DataFolder + "config.dll", s);
        }

        void ResetJob()
        {
            if (watcher.IsRunning)
                watcher.Stop();

            txtBackImage.Text = "";
            txtOverlay.Text = "";
            txtFrameWidth.Text = "0";
            txtFrameHeight.Text = "0";
            txtStickerWidth.Text = "0";
            txtStickerHeight.Text = "0";
            txtStickerCols.Text = "0";
            txtStickerRows.Text = "0";
            frameBorder.Visibility = Visibility.Collapsed;
            imgOverlay.Source = null;
            imgFrame.Source = null;
            imgOverlay.Visibility = Visibility.Collapsed;
            txtTotalPhotosPerPrint.Text = "1";
            if (cboBlendMode.Items.Count >= 1) { cboBlendMode.SelectedIndex = 0; }
            txtBlending.Text = "0";

            // Blending Options
            txtBlend.Number = 35;
            txtContrast.Number = 100;
            txtBrightness.Number = 100;

            lblTotalCells.Content = "0";
            lblTotalPhotos.Content = "0";
            chkFillMosaic.IsChecked = false;
            chkImmediatePrint.IsChecked = true;

            chkPrint.IsChecked = true;
            chkWatcher.IsChecked = false;
            txtSource.Text = "";
            if (cboPrinters.Items.Count >= 1) { cboPrinters.SelectedIndex = 0; }

            pnlBasic.Tag = "1";
            pnlBlendingOptions.Tag = "0";
            pnlDirectoryWatcher.Tag = "0";
            pnlManualProcessing.Tag = "0";
            pnlPrint.Tag = "0";
            pnlProjection.Tag = "0";
            pnlLocalServer.Tag = "0";
            txtTotalPhotosPerPrint.Text = "1";

            pnlBasic.Visibility = Visibility.Visible;
            pnlBlendingOptions.Visibility = Visibility.Collapsed;
            pnlDirectoryWatcher.Visibility = Visibility.Collapsed;
            pnlManualProcessing.Visibility = Visibility.Collapsed;
            pnlStudio94Service.Visibility = Visibility.Collapsed;
            pnlProjection.Visibility = Visibility.Collapsed;
            //pnlLocalServer.Visibility = Visibility.Collapsed;
            pnlPrint.Visibility = Visibility.Collapsed;

            // recreate all folders
            string sourcesFolder = Globals.DataFolder + "sources\\";
            string overlaysFolder = Globals.DataFolder + "overlays\\";
            string filteredFolder = Globals.DataFolder + "filtered\\";
            string processedFolder = Globals.DataFolder + "processed\\";
            if (Directory.Exists(sourcesFolder)) { Directory.Delete(sourcesFolder, true); }
            if (Directory.Exists(overlaysFolder)) { Directory.Delete(overlaysFolder, true); }
            if (Directory.Exists(filteredFolder)) { Directory.Delete(filteredFolder, true); }
            if (Directory.Exists(processedFolder)) { Directory.Delete(processedFolder, true); }

            Directory.CreateDirectory(sourcesFolder);
            Directory.CreateDirectory(overlaysFolder);
            Directory.CreateDirectory(filteredFolder);
            Directory.CreateDirectory(processedFolder);
        }

        private void btnNewJob_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            //MessageBox.Show("New Job Button Clicked");
            if (!jobSaved)
            {
                frmMessageBox msgbox = new MosaicUtility.frmMessageBox("You have Changes that are not Saved Yet.\nDo you want to Save Changes & Close?", "Saving Changes", MessageBoxType.Confirm);
                msgbox.ShowDialog();
                if (msgbox.result == MessageBoxResult.OK)
                {
                    // save changes and start new job
                }
                else if (msgbox.result == MessageBoxResult.Cancel)
                {
                    // dont save changes and start new job
                }
                else
                {
                    // do nothing
                    return;
                }
            }
            ResetJob();
        }

        private void btnOpenJob_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            MessageBox.Show("Open Job Button Clicked");
        }

        private void btnChooseBackImage_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            //dialog.Filter = "All JPG Files (*.jpg) | *.jpg | ALL JPEG Files (*.jpeg) | *.jpg | ALL PNG Files (*.png) | *.png";
            dialog.Title = "Choose Back Image";
            dialog.ShowDialog();
            if (!String.IsNullOrEmpty(dialog.FileName))
            {
                txtBackImage.Text = dialog.FileName;
                if (System.IO.File.Exists(txtBackImage.Text))
                {
                    BitmapImage img = new BitmapImage(new Uri(txtBackImage.Text));
                    imgFrame.Source = img;
                    lblResolution.Content = img.PixelWidth.ToString() + " x " + img.PixelHeight.ToString();
                    //imgFrame.Width = img.PixelWidth;
                    //imgFrame.Height = img.PixelHeight;
                }
            }
        }

        private void btnChooseOverlay_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            //dialog.Filter = "All JPG Files (*.jpg) | *.jpg | ALL JPEG Files (*.jpeg) | *.jpg | ALL PNG Files (*.png) | *.png";
            dialog.Title = "Choose Overlay Image";
            dialog.ShowDialog();
            if (!String.IsNullOrEmpty(dialog.FileName))
            {
                txtOverlay.Text = dialog.FileName;
                imgOverlay.Visibility = Visibility.Visible;
                imgOverlay.Source = new BitmapImage(new Uri(txtOverlay.Text));
            }
        }

        private void txtFrameWidth_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!RegexHelper.ValidateNumeric(txtFrameWidth.Text, txtFrameWidth.Tag.ToString()))
            {
                txtFrameWidth.Text = txtFrameWidth.Text.Remove(txtFrameWidth.Text.Length - 1, 1);
                txtFrameWidth.SelectionStart = txtFrameWidth.Text.Length;
                //return;
            }

            frameBorder.Visibility = Visibility.Collapsed;
            if (string.IsNullOrEmpty(txtFrameWidth.Text))
                return;

            if (!string.IsNullOrEmpty(txtFrameHeight.Text))
            {
                frameBorder.Visibility = Visibility.Visible;
                frameBorder.Width = Convert.ToDouble(txtFrameWidth.Text) * Globals.DPI;
                frameBorder.Height = Convert.ToDouble(txtFrameHeight.Text) * Globals.DPI;
            }

            lblFrameSize.Content = txtFrameWidth.Text.ToString() + " x " + txtFrameHeight.Text.ToString();
            lblResolution.Content = frameBorder.Width.ToString() + " x " + frameBorder.Width.ToString();

        }

        private void txtFrameHeight_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!RegexHelper.ValidateNumeric(txtFrameHeight.Text, "Frame Height"))
            {
                txtFrameHeight.Text = txtFrameHeight.Text.Remove(txtFrameHeight.Text.Length - 1, 1);
                txtFrameHeight.SelectionStart = txtFrameHeight.Text.Length;
                //return;
            }

            frameBorder.Visibility = Visibility.Collapsed;
            if (string.IsNullOrEmpty(txtFrameHeight.Text))
                return;

            if (!string.IsNullOrEmpty(txtFrameWidth.Text))
            {
                frameBorder.Visibility = Visibility.Visible;
                frameBorder.Width = Convert.ToDouble(txtFrameWidth.Text) * Globals.DPI;
                frameBorder.Height = Convert.ToDouble(txtFrameHeight.Text) * Globals.DPI;
            }

            lblFrameSize.Content = txtFrameWidth.Text.ToString() + " x " + txtFrameHeight.Text.ToString();
            lblResolution.Content = frameBorder.Width.ToString() + " x " + frameBorder.Height.ToString();
        }

        void CalculateResult()
        {
            if (string.IsNullOrEmpty(txtFrameWidth.Text))
                txtFrameWidth.Text = "0";

            if (string.IsNullOrEmpty(txtFrameHeight.Text))
                txtFrameHeight.Text = "0";

            if (string.IsNullOrEmpty(txtStickerWidth.Text))
                txtStickerWidth.Text = "0";

            if (string.IsNullOrEmpty(txtStickerHeight.Text))
                txtStickerHeight.Text = "0";

            if (string.IsNullOrEmpty(txtStickerCols.Text))
                txtStickerCols.Text = "0";

            if (string.IsNullOrEmpty(txtStickerRows.Text))
                txtStickerRows.Text = "0";

            totalCols = Convert.ToInt32(txtStickerCols.Text);
            totalRows = Convert.ToInt32(txtStickerRows.Text);

            wall = new Size(Convert.ToDouble(txtFrameWidth.Text), Convert.ToDouble(txtFrameHeight.Text));
            sticker = new Size(Convert.ToDouble(txtStickerWidth.Text), Convert.ToDouble(txtStickerHeight.Text));

            if (wall != new Size(0, 0))
            {
                // calculate cells from wall and sticker size
                if (sticker != new Size(0, 0))
                {
                    Size cell = new Size(wall.Width / sticker.Width, wall.Height / sticker.Height);
                    if (double.IsInfinity(cell.Width))
                        cell.Width = 0;

                    if (double.IsInfinity(cell.Height))
                        cell.Height = 0;

                    totalCols = Convert.ToInt32(cell.Width);
                    totalRows = Convert.ToInt32(cell.Height);
                    txtStickerCols.Text = totalCols.ToString();
                    txtStickerRows.Text = totalRows.ToString();
                }
                else
                {
                    // calculate sticker size from wall and colsxrows
                    sticker = new Size(wall.Width * totalCols, wall.Height * totalRows);
                    txtStickerWidth.Text = sticker.Width.ToString();
                    txtStickerHeight.Text = sticker.Height.ToString();
                }
            }
            else
            {
                if (sticker != new Size(0, 0))
                {

                    wall = new Size(sticker.Width * totalCols, sticker.Height * totalRows);
                    txtFrameWidth.Text = wall.Width.ToString(); ;
                    txtFrameHeight.Text = wall.Height.ToString();
                }
            }

            lblTotalCells.Content = (totalCols * totalRows).ToString();
            if (totalCols > 0 && totalRows > 0)
            {
                gridStickers.ColumnDefinitions.Clear();
                gridStickers.RowDefinitions.Clear();

                for (int x = 0; x < totalRows; x++)
                {
                    RowDefinition row = new RowDefinition();
                    //row.Height = GridLength.
                    gridStickers.RowDefinitions.Add(row);
                }

                for (int y = 0; y < totalCols; y++)
                {
                    ColumnDefinition col = new ColumnDefinition();
                    gridStickers.ColumnDefinitions.Add(col);
                }

                gridStickers.Visibility = Visibility.Visible;
            }
        }

        private void txtFrameWidth_LostFocus(object sender, RoutedEventArgs e)
        {
            if (!RegexHelper.IsTextAllowed(txtFrameWidth.Text))
                return;

            CalculateResult();
        }

        private void txtFrameHeight_LostFocus(object sender, RoutedEventArgs e)
        {
            if (!RegexHelper.IsTextAllowed(txtFrameHeight.Text))
                return;

            CalculateResult();
        }

        private void txtStickerWidth_LostFocus(object sender, RoutedEventArgs e)
        {
            if (!RegexHelper.IsTextAllowed(txtStickerWidth.Text))
                return;

            CalculateResult();
        }

        private void txtStickerHeight_LostFocus(object sender, RoutedEventArgs e)
        {
            if (!RegexHelper.IsTextAllowed(txtStickerHeight.Text))
                return;

            CalculateResult();
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox tb = (TextBox)sender;
            tb.SelectAll();


        }

        private void btnChooseSource_Click(object sender, RoutedEventArgs e)
        {
            txtSource.Text = Classes.WinFormHelper.OpenFolderDialog();
        }

        private void imgCollapseDirectoryWatcher_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (pnlDirectoryWatcher.Tag.ToString() == "1")
            {
                pnlDirectoryWatcher.Visibility = Visibility.Collapsed;
                pnlDirectoryWatcher.Tag = "0";
            }
            else
            {
                pnlDirectoryWatcher.Visibility = Visibility.Visible;
                pnlDirectoryWatcher.Tag = "1";
            }
        }

        private void imgCollapseManualProcessing_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (pnlManualProcessing.Tag.ToString() == "1")
            {
                pnlManualProcessing.Visibility = Visibility.Collapsed;
                pnlManualProcessing.Tag = "0";
            }
            else
            {
                pnlManualProcessing.Visibility = Visibility.Visible;
                pnlManualProcessing.Tag = "1";
            }
        }

        private void imgCollapsePrintingOptions_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (pnlPrint.Tag.ToString() == "1")
            {
                pnlPrint.Visibility = Visibility.Collapsed;
                pnlPrint.Tag = "0";
            }
            else
            {
                pnlPrint.Visibility = Visibility.Visible;
                pnlPrint.Tag = "1";
            }
        }

        private void imgCollapseBasicSettings_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (pnlBasic.Tag.ToString() == "1")
            {
                pnlBasic.Visibility = Visibility.Collapsed;
                pnlBasic.Tag = "0";
            }
            else
            {
                pnlBasic.Visibility = Visibility.Visible;
                pnlBasic.Tag = "1";
            }
        }

        private void btnSelectPhotos_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            //dialog.Filter = "*.jpg";
            dialog.Multiselect = true;
            dialog.Title = "Select Photos";
            if (dialog.ShowDialog() == true)
            {
                photos = dialog.FileNames;
                int count = photos.Length;
                lblTotalPhotos.Content = count.ToString();
            }
        }

        private void btnProcessManual_Click(object sender, RoutedEventArgs e)
        {
            bool repeatMode = (bool)chkFillMosaic.IsChecked;
            bool immediatePrint = (bool)chkImmediatePrint.IsChecked;
            int totalCells = Convert.ToInt32(lblTotalCells.Content);

            pbManual.Minimum = 0;
            pbManual.Maximum = photos.Length;
            pbManual.Value = 0;

            Task.Factory.StartNew(() =>
            {
                int a = 0;
                while (photos.Length < totalCells)
                {
                    string file = photos[a];
                    photos[photos.Length + 1] = file;
                }

                for (int i = 0; i < photos.Length; i++)
                {
                    try
                    {
                        var photo = photos[i];
                        string filtered = mosaic.ProcessRandom(photo, new System.Drawing.Size((int)sticker.Width * Globals.DPI, (int)sticker.Height * Globals.DPI));
                        //if (copies == true)
                        //{
                        //    File.Copy(filtered, filtered.Replace("filtered", "copies"));
                        //}
                        var counts = Directory.GetFiles(Globals.DataFolder + "sources\\").Length;
                        if (counts == 0)
                        {
                            break;
                        }

                        Dispatcher.Invoke(new Action(() =>
                        {
                            pbManual.Value++;
                            if ((bool)chkPrint.IsChecked)
                            {
                                if (immediatePrint)
                                {
                                    if (filtered != null)
                                        Globals.PrintPhoto(filtered, null,sticker);
                                }
                            }
                        }));
                    }
                    catch (Exception ex)
                    {
                        new frmMessageBox(ex.Message, "Mosaic Utility", MessageBoxType.Alert).ShowDialog();
                    }
                }

                Dispatcher.Invoke(new Action(() =>
                {
                    pbManual.Value = 100;
                    new frmMessageBox("Process Completed Successfully ...", "Mosaic Utility", MessageBoxType.Info).ShowDialog();
                    lblTotalPhotos.Content = "0";
                    // show slices in grid
                    ShowSlices();
                }));
            });
        }

        private void ShowSlices()
        {
            string dir = Globals.DataFolder + "filtered\\";
            string[] files = Directory.GetFiles(dir);
            for (int i = 0; i < totalRows; i++)
            {
                for (int j = 0; j < totalCols; j++)
                {
                    string filePath = dir + "R{0}_C{1}.png";
                    filePath = string.Format(filePath, (i + 1).ToString(), (j + 1).ToString());
                    ShowFiltered(filePath, i, j);
                }

            }
        }

        private void ShowFiltered(string filePath, int row, int col)
        {
            Dispatcher.Invoke(() =>
            {
                if (File.Exists(filePath))
                {
                    Image img = new Image();
                    img.Source = new BitmapImage(new Uri(filePath));
                    img.Stretch = Stretch.Fill;
                    Grid.SetColumn(img, col);
                    Grid.SetRow(img, row);
                    gridStickers.Children.Add(img);
                }
            });


        }

        private void txtBackImage_TextChanged(object sender, TextChangedEventArgs e)
        {

        }


        private void btnPrepareBackImage_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtBackImage.Text))
            {
                new frmMessageBox("Please Select Back Image First to Process...", "Mosaic Utility", MessageBoxType.Alert).ShowDialog();
                return;
            }

            if (string.IsNullOrEmpty(txtFrameWidth.Text))
            {
                new frmMessageBox("Please Enter Wall Width Properly...", "Mosaic Utility", MessageBoxType.Alert).ShowDialog();
                return;
            }

            if (string.IsNullOrEmpty(txtFrameHeight.Text))
            {
                new frmMessageBox("Please Enter Wall Height Properly...", "Mosaic Utility", MessageBoxType.Alert).ShowDialog();
                return;
            }

            if (string.IsNullOrEmpty(txtStickerWidth.Text))
            {
                new frmMessageBox("Please Enter Sticker Width Properly...", "Mosaic Utility", MessageBoxType.Alert).ShowDialog();
                return;
            }

            if (string.IsNullOrEmpty(txtStickerHeight.Text))
            {
                new frmMessageBox("Please Enter Sticker Height Properly...", "Mosaic Utility", MessageBoxType.Alert).ShowDialog();
                return;
            }

            string directory = Globals.DataFolder + "sources\\";
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            else
            {
                if (Directory.GetFiles(directory).Length > 0)
                {
                    frmMessageBox msgbox = new frmMessageBox("There are Pending Files in the Sources Folder.\n Do you want to Remove those files?", "Pending Job", MessageBoxType.Confirm);
                    msgbox.ShowDialog();
                    if (msgbox.result == MessageBoxResult.OK)
                    {
                        Directory.Delete(directory, true);
                        new frmMessageBox("Directory Cleared Successfully ...\nPress Again to Start Your Process...", "Mosaic Utility", MessageBoxType.Info).ShowDialog();
                    }
                    return;
                }
            }

            bSliceImage = Globals.BitmapFromSource((BitmapSource)imgFrame.Source.Clone());
            if (bSliceImage == null)
            {
                frmMessageBox msgbox = new frmMessageBox("Select Back Image First.", "Image Not Found", MessageBoxType.Alert);
                msgbox.ShowDialog();
                return;
            }

            sliceWidth = Convert.ToInt32(txtStickerWidth.Text) * Globals.DPI;
            sliceHeight = Convert.ToInt32(txtStickerHeight.Text) * Globals.DPI;
            //sliceHeight = bSliceImage.Height/totalRows;


            pbSlicer.Minimum = 0;
            pbSlicer.Maximum = totalCols * totalRows;
            List<string> data = new List<string>();
            int totalImages = 0;

            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += (ss, ee) =>
            {
                SD.Bitmap bitmap = new SD.Bitmap(sliceWidth, sliceHeight);
                SD.Graphics g = SD.Graphics.FromImage(bitmap);
                int x = 0;
                int y = 0;
                for (int i = 0; i < totalRows; i++)
                {
                    x = 0;
                    for (int j = 0; j < totalCols; j++)
                    {
                        var name = "R" + (i + 1) + "_C" + (j + 1);
                        var p = new SD.Point(j * sliceWidth, i * sliceHeight);
                        string path = directory + name + ".png";
                        g.DrawImage(bSliceImage, new SD.Rectangle(0, 0, bitmap.Width, bitmap.Height),
                           new SD.Rectangle(p.X, p.Y, sliceWidth, sliceHeight), SD.GraphicsUnit.Pixel);

                        bitmap.Save(path, SDI.ImageFormat.Png);
                        g.Clear(System.Drawing.Color.Transparent);
                        totalImages++;
                        //x += sliceWidth;
                        worker.ReportProgress(0);
                    }
                    //y += sliceHeight;
                }

                g.Dispose();
                bitmap.Dispose();
            };

            worker.RunWorkerCompleted += (ss, ee) =>
            {
                pbSlicer.Visibility = Visibility.Collapsed;
                File.WriteAllLines("log.txt", data);
                bSliceImage.Dispose();
                new frmMessageBox(totalImages + " Slices Saved", "Slicing", MessageBoxType.Info).ShowDialog();

            };

            worker.ProgressChanged += (ss, ee) =>
            {
                pbSlicer.Value = totalImages;
            };

            pbSlicer.Visibility = Visibility.Visible;
            worker.WorkerReportsProgress = true;
            worker.RunWorkerAsync();

        }

        private void btnPrepareOverlay_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtOverlay.Text))
            {
                new frmMessageBox("Please Select An Image for the Overlay First.", "Mosaic Utility", MessageBoxType.Alert).ShowDialog();
                txtOverlay.Focus();
                return;
            }

            string directory = Globals.DataFolder + "overlays\\";
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            else
            {
                if (Directory.GetFiles(directory).Length > 0)
                {
                    frmMessageBox msgbox = new frmMessageBox("There are Pending Files in the Overlays Folder.\n Do you want to Remove those files?", "Pending Job", MessageBoxType.Confirm);
                    msgbox.ShowDialog();
                    if (msgbox.result == MessageBoxResult.OK)
                    {
                        Directory.Delete(directory, true);
                        new frmMessageBox("Directory Cleared Successfully ...\nPress Again to Start Your Process...", "Mosaic Utility", MessageBoxType.Info).ShowDialog();
                    }
                    return;
                }
            }

            bSliceImage = new SD.Bitmap(txtOverlay.Text);
            if (bSliceImage == null)
            {
                frmMessageBox msgbox = new frmMessageBox("Select Overlay Image First.", "Image Not Found", MessageBoxType.Alert);
                msgbox.ShowDialog();
                return;
            }

            sliceWidth = Convert.ToInt32(txtStickerWidth.Text) * Globals.DPI;
            sliceHeight = Convert.ToInt32(txtStickerHeight.Text) * Globals.DPI;

            pbSlicer.Minimum = 0;
            pbSlicer.Maximum = totalCols * totalRows;
            List<string> data = new List<string>();
            int totalImages = 0;

            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += (ss, ee) =>
            {
                SD.Bitmap bitmap = new SD.Bitmap(sliceWidth, sliceHeight);
                SD.Graphics g = SD.Graphics.FromImage(bitmap);
                int x = 0;
                int y = 0;
                for (int i = 0; i < totalRows; i++)
                {
                    x = 0;
                    for (int j = 0; j < totalCols; j++)
                    {
                        var name = "R" + (i + 1) + "_C" + (j + 1);
                        var p = new SD.Point(j * sliceWidth, i * sliceHeight);
                        string path = directory + name + ".png";
                        g.DrawImage(bSliceImage, new SD.Rectangle(0, 0, bitmap.Width, bitmap.Height),
                           new SD.Rectangle(p.X, p.Y, sliceWidth, sliceHeight), SD.GraphicsUnit.Pixel);

                        bitmap.Save(path, SDI.ImageFormat.Png);
                        g.Clear(System.Drawing.Color.Transparent);
                        totalImages++;
                        //x += sliceWidth;
                        worker.ReportProgress(0);
                    }
                    //y += sliceHeight;
                }

                g.Dispose();
                bitmap.Dispose();
            };

            worker.RunWorkerCompleted += (ss, ee) =>
            {
                pbSlicer.Visibility = Visibility.Collapsed;
                File.WriteAllLines("log.txt", data);
                bSliceImage.Dispose();
                new frmMessageBox(totalImages + " Slices Saved", "Slicing", MessageBoxType.Info).ShowDialog();

            };

            worker.ProgressChanged += (ss, ee) =>
            {
                pbSlicer.Value = totalImages;
            };

            pbSlicer.Visibility = Visibility.Visible;
            worker.WorkerReportsProgress = true;
            worker.RunWorkerAsync();
        }

        #region DirectoryWatcher
        BackgroundWorker downloader = new BackgroundWorker();
        List<int> eventCodes = new List<int>();

        void InitDownloader()
        {
            try
            {
                downloader.DoWork += Downloader_DoWork; ; ;
                downloader.RunWorkerCompleted += new RunWorkerCompletedEventHandler(Downloader_RunWorkerCompleted);
                downloader.ProgressChanged += new ProgressChangedEventHandler(Downloader_ProgressChanged);
                downloader.WorkerReportsProgress = true;
            }
            catch (Exception ex)
            {
                File.AppendAllLines("error.log", new string[] { "Thread Start Error : " + ex.Message });
            }
        }

        private void Downloader_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            string content = e.UserState.ToString();

            int counter = e.ProgressPercentage;
            try
            {

                if (!content.Contains("FILE_DOWNLOADED"))
                {
                    string fileName = e.UserState.ToString();
                    string[] splits = fileName.Split('\\');
                    string fname = splits[splits.Length - 1];
                    string ext = System.IO.Path.GetExtension(fileName);

                    // move file to the directory watcher on downloading completed
                    if (!string.IsNullOrEmpty(downloadFolder))
                        File.Move(fileName, downloadFolder+fname);

                }


            }
            catch (Exception ex)
            {

            }
        }

        private void Downloader_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            try
            {
                if (e.Result != null && e.Result.ToString().Contains("error"))
                {
                    //MessageBox.Show(e.Result.ToString());
                }
                btnRunService.Content = "Start Service";
                dw.Stop();

            }
            catch (Exception)
            {

            }
        }

        int totalFiles = 0;
        bool processStoped = false;
        private void Downloader_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                List<EventData> data = GetEventData();
                string folderpath = Globals.DataFolder + "temp\\";
                Globals.CreateFolder(folderpath);
                //e.Result = data;

                totalFiles = data.Count;
                int counter = 0;

                //download each image in local temp folder
                foreach (EventData item in data)
                {

                    if (processStoped)
                    {
                 
                        break;
                    }

                    counter++;
                    // make file name
                    string localFilePath = "";
                    string[] splits = item.ServerPath.Split('/');
                    string fname = splits[splits.Length - 1];
                    string fileName = folderpath + fname;

                    if (!File.Exists(fileName))
                    {
                        string ext = System.IO.Path.GetExtension(item.ServerPath);
                        Dispatcher.Invoke(() =>
                        {
                            pbDownloader.Visibility = System.Windows.Visibility.Visible;
                        });

                        //new ApiHelper().DownloadItem(itemUrl, _localPath);
                        // apply file format filter

                        localFilePath = DownloadFile(item.ServerPath, fileName);

                        // logging
                        //var json = SimpleJson.SerializeObject(userdata);
                        //File.WriteAllText("userdata.json", json);
                        if (!string.IsNullOrEmpty(localFilePath))
                        {
                            downloader.ReportProgress(counter, localFilePath);
                            System.Threading.Thread.Sleep(1000);
                        }
                    }
                    else
                    {
                        downloader.ReportProgress(counter, "FILE_DOWNLOADED");
                    }
                }
                System.Threading.Thread.Sleep(10000);
            }
            catch (Exception ex)
            {
                e.Result = "error: " + ex.Message;
            }
        }

        List<EventData> GetEventData()
        {
            List<EventData> result = new List<EventData>();
            foreach (int eventCode in eventCodes)
            {
                //List<EventData> result = GetImages(eventCode);

                var eventItems = new ApiHelper(Globals.APIUrl).GetData("/GetImageList/", new EventData() { EventID = eventCode, ID = Convert.ToInt32(0) });
                if (eventItems != null)
                {
                    result.AddRange(eventItems as List<EventData>);
                }

            }

            return result;
        }

        List<EventData> GetImages(int EventID)
        {
            try
            {
                var client = new RestClient(Globals.APIUrl);

                RestRequest request = new RestRequest("/GetImageList/", Method.POST);
                request.AddJsonBody(new EventData() { EventID = EventID, ID = Convert.ToInt32(0) });
                var response = client.Execute<List<EventData>>(request);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var data = response.Data;
                    return data;
                }
            }
            catch (Exception ex)
            {
                File.AppendAllLines("error.log", new string[] { "Get Images error : " + ex.Message });
            }

            return new List<EventData>();
        }

        bool downloaded = false;
        string DownloadFile(string url, string localPath)
        {
            try
            {

                downloaded = false;

                //string name = url.Split('/').Last();
                //string path = AppDomain.CurrentDomain.BaseDirectory + "downloads\\" + name.Replace(".mp4", "_.mp4");
                if (!File.Exists(localPath))
                {
                    WebClient client = new WebClient();
                    client.DownloadFileCompleted += Client_DownloadFileCompleted;
                    client.DownloadProgressChanged += Client_DownloadProgressChanged;
                    client.DownloadFileAsync(new Uri(url), localPath);
                    while (!downloaded)
                    {
                        System.Threading.Thread.Sleep(1);
                    }

                    client.Dispose();
                }

                return localPath;
            }
            catch (Exception)
            {
                return "";
            }
        }

        private void Client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                pbDownloader.Value = e.ProgressPercentage;
            });
        }

        private void Client_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                pbDownloader.Visibility = System.Windows.Visibility.Collapsed;
            });

            downloaded = true;
        }

        private void btnTestUrl_Click(object sender, RoutedEventArgs e)
        {
            Ping x = new Ping();
            PingReply reply = x.Send(IPAddress.Parse(txtApiUrl.Text));

            if (reply.Status == IPStatus.Success)
                new frmMessageBox("Success: \nServer is Accessible", "Checking API", MessageBoxType.Info).ShowDialog();
            else
                new frmMessageBox("Failure: \nServer is not Accessible", "Checking API", MessageBoxType.Info).ShowDialog();
        }

        string downloadFolder = "";
        DirectoryWatcher dw = null;

        private void btnRunService_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtEventCodes.Text))
            {
                new frmMessageBox("Please Enter Event Codes (comma separted if multiple) to Run the service", "Mosaic Utility", MessageBoxType.Alert).ShowDialog();
                return;
            }

            if (btnRunService.Content.ToString().Contains("Run"))
            {
                txtEventCodes.IsEnabled = false;
                // start downloader thread
                Globals.APIUrl = txtApiUrl.Text;
                downloadFolder = Globals.DataFolder + "downloads\\";
                Globals.CreateFolder(downloadFolder);

                InitDownloader();

                eventCodes = new List<int>();

                var splits = txtEventCodes.Text.Split(',').ToList();
                foreach (var item in splits)
                {
                    try
                    {
                        eventCodes.Add(Convert.ToInt32(item));
                    }
                    catch (Exception ex)
                    {
                        File.AppendAllLines("error.log", new string[] { "Seach error : " + ex.Message });
                    }
                }

                // start a directory watcher
                
                dw = new DirectoryWatcher(downloadFolder);
                dw.OnNewFile += (ss, ee) =>
                {
                    string fileName = ss.ToString();
                    //fileName = watcher.getNewFile();
                    ProcessFile(fileName);
                };
                dw.Start();

                processStoped = false;
                downloader.RunWorkerAsync();

                btnRunService.Content = "Stop";
            }
            else
            {
                processStoped = true;
                txtEventCodes.IsEnabled = true;
                btnRunService.Content = "Stopping ...";
            }
        }

        private void txtApiUrl_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (lblUrlPort == null)
                return;


            if (!string.IsNullOrEmpty(txtApiUrl.Text))
                lblUrlPort.Visibility = Visibility.Collapsed;
            else
                lblUrlPort.Visibility = Visibility.Visible;
        }

        private void txtEventCodes_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(txtEventCodes.Text))
                lblEnterCodesHint.Visibility = Visibility.Collapsed;
            else
                lblEnterCodesHint.Visibility = Visibility.Visible;
        }

        private void btnStartService_Click(object sender, RoutedEventArgs e)
        {
            if (btnStartService.Content.ToString().Equals("Start"))
            {
                if (string.IsNullOrEmpty(txtSource.Text))
                {
                    new frmMessageBox("Please Select Source Directory to Start Service On", "Mosaic Utility", MessageBoxType.Alert).ShowDialog();
                    return;
                }

                watcher = new DirectoryWatcher(txtSource.Text);
                watcher.OnNewFile += (ss, ee) =>
                {
                    string fileName = ss.ToString();
                    //fileName = watcher.getNewFile();
                    ProcessFile(fileName);
                };
                btnStartService.Content = "Stop";
                watcher.Start();
            }
            else
            {
                watcher.Stop();
                btnStartService.Content = "Start";
            }
        }

        #endregion

        void ProcessFile(string photo)
        {
            try
            {
                string filtered = mosaic.ProcessRandom(photo, new System.Drawing.Size((int)sticker.Width * Globals.DPI, (int)sticker.Height * Globals.DPI));
                if (filtered == "")
                {
                    if (mosaic.NoMoreSlices)
                    {
                        new frmMessageBox("No More Source Found...", "Mosaic Utility", MessageBoxType.Alert).ShowDialog();
                        if (!processStoped)
                        {
                            btnRunService_Click(null, null);
                        }
                    }
                }
                else
                {
                    Globals.SlideshowAlbum.Add(filtered);
                    var refined = System.IO.Path.GetFileName(filtered).Replace("C", "").Replace("R", "").Replace(".jpg", "").Replace(".png", "");
                    var row = Convert.ToInt32(refined.Split('_').First().Trim()) - 1;
                    var col = Convert.ToInt32(refined.Split('_').Last().Trim()) - 1;
                    ShowFiltered(filtered, row, col);

                    if ((bool)chkPrint.IsChecked)
                    {
                        if ((bool)chkImmediatePrint.IsChecked)
                        {
                            if (filtered != null)
                                Globals.PrintPhoto(filtered, null, sticker);
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }

        private void cboBlendMode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var name = (BlendingMode)Enum.Parse(typeof(BlendingMode), cboBlendMode.SelectedValue.ToString());
            mosaic.BlendName = name;

        }

        private void imgCollapseBlendingOptions_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (pnlBlendingOptions.Tag.ToString() == "1")
            {
                pnlBlendingOptions.Visibility = Visibility.Collapsed;
                pnlBlendingOptions.Tag = "0";
            }
            else
            {
                pnlBlendingOptions.Visibility = Visibility.Visible;
                pnlBlendingOptions.Tag = "1";
            }
        }

        private void txtBlend_OnValueChanged(object sender, EventArgs e)
        {
            mosaic.BlendValue = Convert.ToSingle(txtBlend.Number / 100f);
        }

        private void txtBrightness_OnValueChanged(object sender, EventArgs e)
        {
            mosaic.Brightness = Convert.ToSingle(txtBrightness.Number / 100f);
        }

        private void txtContrast_OnValueChanged(object sender, EventArgs e)
        {
            mosaic.Contrast = Convert.ToSingle(txtContrast.Number / 100f);
        }

        private void btnReset_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {

            frmMessageBox msgbox = new MosaicUtility.frmMessageBox("This will Reset the Job & Remove All the Resources Related to this Job. It may cause Your Work to Lost.\nDo you want to continue?", "Mosaic Utility", MessageBoxType.Confirm);
            msgbox.ShowDialog();
            if (msgbox.result == MessageBoxResult.OK)
            {
                // save changes and start new job
            }
            else if (msgbox.result == MessageBoxResult.Cancel)
            {
                // dont save changes and start new job
            }
            else
            {
                // do nothing
                return;
            }

            ResetJob();
        }

        private void btnExit_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            //SaveConfig();            
            this.Close();
            Application.Current.Shutdown();
        }

        private void txtStickerWidth_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!RegexHelper.ValidateNumeric(txtStickerWidth.Text, txtStickerWidth.Tag.ToString()))
            {
                txtStickerWidth.Text = txtStickerWidth.Text.Remove(txtStickerWidth.Text.Length - 1, 1);
                txtStickerWidth.SelectionStart = txtStickerWidth.Text.Length;
                //return;
            }
        }

        private void txtStickerHeight_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!RegexHelper.ValidateNumeric(txtStickerHeight.Text, txtStickerHeight.Tag.ToString()))
            {
                txtStickerHeight.Text = txtStickerHeight.Text.Remove(txtStickerHeight.Text.Length - 1, 1);
                txtStickerHeight.SelectionStart = txtStickerHeight.Text.Length;
                //return;
            }
        }

        private void btnMakePrinterDefault_Click(object sender, RoutedEventArgs e)
        {
            string printerName = cboPrinters.Text;
            if (PrinterClass.SetDefaultPrinter(printerName))
            {
                new frmMessageBox(printerName + " Set to Default Printer Successfully ...", "Mosaic Utility", MessageBoxType.Info).ShowDialog();
            }else
            {
                new frmMessageBox(printerName + " Not Set to Default Printer. \nTry Setting in the Control Panel Manually", "Mosaic Utility", MessageBoxType.Alert).ShowDialog();
            }
        }

        private void imgCollapseS94Service_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (pnlStudio94Service.Tag.ToString() == "1")
            {
                pnlStudio94Service.Visibility = Visibility.Collapsed;
                pnlStudio94Service.Tag = "0";
            }
            else
            {
                pnlStudio94Service.Visibility = Visibility.Visible;
                pnlStudio94Service.Tag = "1";
            }
        }

        private void lblEnterCodesHint_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            txtEventCodes.Focus();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            SaveConfig();
        }

        private void lblScreenshow_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            frmScreen f = new MosaicUtility.frmScreen();
            f.ShowDialog();
        }

        private void lblSlideshow_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            frmSlideshow f = new MosaicUtility.frmSlideshow();
            f.ShowDialog();
        }

        WebServer server;
        private void btnRunLocalServer_Click(object sender, RoutedEventArgs e)
        {
            if (btnRunLocalServer.Content.ToString().Contains("Start"))
            {
                server = new WebServer(txtLocalFolder.Text, Convert.ToInt32(txtLocalServePort.Text));
                if (server.Port > 0)
                {
                    btnRunLocalServer.Content = "Stop";
                    //// Fire up the browser to show the content!
                    //var browser = new Process
                    //{
                    //    StartInfo = new ProcessStartInfo("http://localhost:8484/")
                    //    {
                    //        UseShellExecute = true
                    //    }
                    //};

                    //browser.Start();
                }
            }
            else
            {
                server.Stop();
            }
        }

        private void btnChooseLocalFolder_Click(object sender, RoutedEventArgs e)
        {
            txtLocalFolder.Text = Classes.WinFormHelper.OpenFolderDialog();
        }

        private void imgCollapseLocalServer_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (pnlLocalServer.Tag.ToString() == "1")
            {
                pnlLocalServer.Visibility = Visibility.Collapsed;
                pnlLocalServer.Tag = "0";
            }
            else
            {
                pnlLocalServer.Visibility = Visibility.Visible;
                pnlLocalServer.Tag = "1";
            }
        }

        private void imgCollapseProjection_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (pnlProjection.Tag.ToString() == "1")
            {
                pnlProjection.Visibility = Visibility.Collapsed;
                pnlProjection.Tag = "0";
            }
            else
            {
                pnlProjection.Visibility = Visibility.Visible;
                pnlProjection.Tag = "1";
            }
        }

        void CalculateSlices()
        {
            if (string.IsNullOrEmpty(txtScreenWidth.Text))
                return;

            if (string.IsNullOrEmpty(txtScreenHeight.Text))
                return;

            if (string.IsNullOrEmpty(txtSliceWidth.Text))
                return;

            if (string.IsNullOrEmpty(txtSliceHeight.Text))
                return;

            Size cell = new Size(Convert.ToInt32(txtScreenWidth.Text) / Convert.ToInt32(txtSliceWidth.Text), Convert.ToInt32(txtScreenHeight.Text) / Convert.ToInt32(txtSliceHeight.Text));

            lblTotalSlices.Content = (cell.Width * cell.Height).ToString();
        }

        private void txtScreenWidth_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!RegexHelper.ValidateNumeric(txtScreenWidth.Text,"Image Width"))
            {
                return;
            }

            CalculateSlices();
        }

        private void txtScreenHeight_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!RegexHelper.ValidateNumeric(txtScreenHeight.Text, "Image Height"))
            {
                return;
            }

            CalculateSlices();
        }

        private void txtSliceWidth_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!RegexHelper.ValidateNumeric(txtSliceWidth.Text, "Image Height"))
            {
                return;
            }

            CalculateSlices();
        }

        private void txtSliceHeight_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!RegexHelper.ValidateNumeric(txtSliceHeight.Text, "Image Height"))
            {
                return;
            }

            CalculateSlices();
        }

        private void btnProject_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtBackImage.Text))
            {
                new frmMessageBox("Back Image Not Set ....", "Mosaic Utility", MessageBoxType.Info).ShowDialog();
                return;
            }

            if (string.IsNullOrEmpty(txtScreenWidth.Text))
                return;

            if (string.IsNullOrEmpty(txtScreenHeight.Text))
                return;

            if (string.IsNullOrEmpty(txtSliceWidth.Text))
                return;

            if (string.IsNullOrEmpty(txtSliceHeight.Text))
                return;

            Size cell = new Size(Convert.ToInt32(txtScreenWidth.Text) / Convert.ToInt32(txtSliceWidth.Text), Convert.ToInt32(txtScreenHeight.Text) / Convert.ToInt32(txtSliceHeight.Text));

            frmProjection f = new MosaicUtility.frmProjection();
            f.ImageSize = new Size(Convert.ToDouble(txtScreenWidth.Text), Convert.ToDouble(txtScreenHeight.Text));
            f.SliceSize = new Size(Convert.ToDouble(txtSliceWidth.Text), Convert.ToDouble(txtSliceHeight.Text));
            f.totalCols = (int)cell.Width;
            f.totalRows = (int)cell.Height;
            f.ImagePath = txtBackImage.Text;
            f.ShowDialog();
        }

        private void lblUrlPort_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            txtLocalServePort.Focus();
        }
    }
}
