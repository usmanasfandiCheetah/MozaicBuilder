using MosaicUtility.Classes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MosaicUtility
{
    /// <summary>
    /// Interaction logic for frmProjection.xaml
    /// </summary>
    public partial class frmProjection : Window
    {
        public string ImagePath = "";
        public Size ImageSize;
        public Size SliceSize;
        public int totalCols = 0;
        public int totalRows = 0;
        DirectoryWatcher watcher;
        
        public frmProjection()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            grdMain.Width = ImageSize.Width;
            grdMain.Height = ImageSize.Height;

            imgMain.Width = ImageSize.Width;
            imgMain.Height = ImageSize.Height;

            if (totalCols > 0 && totalRows > 0)
            {
                grdContents.ColumnDefinitions.Clear();
                grdContents.RowDefinitions.Clear();

                for (int x = 0; x < totalRows; x++)
                {
                    RowDefinition row = new RowDefinition();
                    //row.Height = GridLength.
                    grdContents.RowDefinitions.Add(row);
                }

                for (int y = 0; y < totalCols; y++)
                {
                    ColumnDefinition col = new ColumnDefinition();
                    grdContents.ColumnDefinitions.Add(col);
                }

                grdContents.Visibility = Visibility.Visible;
            }

            if (!string.IsNullOrEmpty(ImagePath))
            {
                if (System.IO.File.Exists(ImagePath))
                {
                    imgMain.Source = new BitmapImage(new Uri(ImagePath, UriKind.RelativeOrAbsolute));
                }
            }

            watcher = new Classes.DirectoryWatcher(Globals.DataFolder + "filtered\\");
            watcher.OnNewFile += (ss, ee) =>
            {
                string fileName = ss.ToString();
                //fileName = watcher.getNewFile();
                ProcessFile(fileName);
            };

            if (!watcher.IsRunning)
                watcher.Start();
        }

        void ProcessFile(string filePath)
        {
            var refined = System.IO.Path.GetFileName(filePath).Replace("C", "").Replace("R", "").Replace(".jpg", "").Replace(".png", "");
            var row = Convert.ToInt32(refined.Split('_').First().Trim()) - 1;
            var col = Convert.ToInt32(refined.Split('_').Last().Trim()) - 1;
            ShowFiltered(filePath, row, col);
        }

        private void ShowFiltered(string filePath, int row, int col)
        {
            Dispatcher.Invoke(() =>
            {
                if (File.Exists(filePath))
                {
                    Image img = new Image();
                    img.Source = new BitmapImage(new Uri(filePath,UriKind.RelativeOrAbsolute));
                    img.Stretch = Stretch.Fill;
                    Grid.SetColumn(img, col);
                    Grid.SetRow(img, row);
                    grdContents.Children.Add(img);
                }
            });


        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (watcher.IsRunning)
                watcher.Stop();
        }
    }
}
