using System;
using System.Collections.Generic;
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
using System.Windows.Threading;

namespace MosaicUtility
{
    /// <summary>
    /// Interaction logic for frmSlideshow.xaml
    /// </summary>
    public partial class frmSlideshow : Window
    {
        int CurrentSlide = 0;
        DispatcherTimer SlideTimer = new DispatcherTimer();

        public frmSlideshow()
        {
            InitializeComponent();
            SlideTimer.Interval = new TimeSpan(0, 0, 1);
            SlideTimer.Tick += SlideTimer_Tick;
        }

        private void SlideTimer_Tick(object sender, EventArgs e)
        {
            LoadImage();
            CurrentSlide++;
            if (CurrentSlide >= Globals.SlideshowAlbum.Count)
            {
                CurrentSlide = 0;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Globals.LoadDemoSlides();
        }

        private void btnPlay_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (btnPlay.Tag.ToString() == "0")
                StartSlideshow();
            else
                StopSlideshow();
        }

        private void imgPrevious_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            CurrentSlide--;
            if (CurrentSlide < 0)
                CurrentSlide = 0;
            LoadImage();
        }

        private void imgNext_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            CurrentSlide++;
            if (CurrentSlide >= Globals.SlideshowAlbum.Count)
                CurrentSlide = 0;
            LoadImage();
        }

        void StartSlideshow()
        {
            if (Globals.SlideshowAlbum.Count == 0)
            {
                new frmMessageBox("No Photos in the Slideshow Gallery to Play...", "Mosaic Printer", MessageBoxType.Alert).ShowDialog();
                return;
            }

            triangle.Visibility = Visibility.Collapsed;
            square.Visibility = Visibility.Visible;
            imgPrevious.IsEnabled = false;
            imgNext.IsEnabled = false;
            btnPlay.Tag = 1;
            SlideTimer.Start();
        }

        void LoadImage()
        {
            imgSlide.Source = new BitmapImage(new Uri(Globals.SlideshowAlbum[CurrentSlide]));
            
        }

        void StopSlideshow()
        {
            SlideTimer.Stop();
            triangle.Visibility = Visibility.Visible;
            square.Visibility = Visibility.Collapsed;
            imgPrevious.IsEnabled = true;
            imgNext.IsEnabled = true;
            btnPlay.Tag = 0;
        }

        private void txtInterval_OnValueChanged(object sender, EventArgs e)
        {
            SlideTimer.Interval = new TimeSpan(0, 0, txtInterval.Number);
        }

        private void pnlClose_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            this.Close();
        }
    }
}
