using MosaicUtility.Classes;
using System;
using System.Collections.Generic;
using SD = System.Drawing;
using SDD = System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MosaicUtility
{
    public class Globals
    {
        public static string APIUrl = "";
        public static string RootFolder = System.AppDomain.CurrentDomain.BaseDirectory;
        public static string DataFolder = System.AppDomain.CurrentDomain.BaseDirectory + "data\\";
        public static int DPI = 200;
        public static List<string> SlideshowAlbum = new List<string>();

        public static void CreateFolder(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        public static void LoadDemoSlides()
        {
            string[] files = Directory.GetFiles(@"F:\Wallpapers\Games Wallpapers\");
            if (files.Length > 0)
            {
                SlideshowAlbum.AddRange(files.ToList());
            }
        }

        public static void WriteErrorLog(string msg)
        {

        }

        public static void TransformToPixels(Visual visual,
                              double unitX,
                              double unitY,
                              out int pixelX,
                              out int pixelY)
        {
            Matrix matrix;
            var source = PresentationSource.FromVisual(visual);
            if (source != null)
            {
                matrix = source.CompositionTarget.TransformToDevice;
            }
            else
            {
                using (var src = new HwndSource(new HwndSourceParameters()))
                {
                    matrix = src.CompositionTarget.TransformToDevice;
                }
            }

            pixelX = (int)(matrix.M11 * unitX);
            pixelY = (int)(matrix.M22 * unitY);
        }

        public static System.Drawing.Bitmap BitmapFromSource(BitmapSource bitmapsource)
        {
            System.Drawing.Bitmap bitmap;
            using (MemoryStream outStream = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();

                enc.Frames.Add(BitmapFrame.Create(bitmapsource));
                enc.Save(outStream);
                bitmap = new System.Drawing.Bitmap(outStream);
            }
            return bitmap;
        }

        public static IDictionary<int, string> ReadEnum<TEnum>() where TEnum : struct
        {
            var enumerationType = typeof(TEnum);

            if (!enumerationType.IsEnum)
                throw new ArgumentException("Enumeration type is expected.");

            var dictionary = new Dictionary<int, string>();

            foreach (int value in Enum.GetValues(enumerationType))
            {
                var name = Enum.GetName(enumerationType, value);
                dictionary.Add(value, name);
            }

            return dictionary;
        }

        public static void PrintPhoto(string filePath, string overlayImage, Size _size)
        {
            SD.Image photo = SD.Image.FromFile(filePath);
            //photo = (SD.Image)frame.Clone();
            //if (photo.Width == 1000)
            //{
            //    photo.RotateFlip(SD.RotateFlipType.Rotate90FlipNone);
            //}
            //photo = resizeImage(photoList[0].Photo, new Size(1500, 1000));

            
            if (overlayImage != null)
            {
                using (SD.Graphics g = SD.Graphics.FromImage(photo))
                {
                    g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                    SD.Image overlay = SD.Image.FromFile(overlayImage);
                    //if (PrintVariables.IsVertical)
                    //{
                    //    photo.RotateFlip(RotateFlipType.Rotate90FlipNone);
                    //}
                    g.DrawImage(overlay, new SD.Rectangle(0, 0, photo.Width, photo.Height));

                    //g.RotateTransform(90.0f);
                    g.Save();
                    g.Dispose();
                }
            }

            //photo.Save("d:\\aa1.jpg", ImageFormat.Jpeg);
            ImagePrinter image_print = new ImagePrinter();
            image_print.width = (int)_size.Width;
            image_print.height = (int)_size.Height;

            //photo.RotateFlip(RotateFlipType.Rotate90FlipNone);
            // Image frame1 = (EmptySingleFrame != "") ? Image.FromFile(EmptySingleFrame) : null;
            image_print.PrintImage(photo, null);
        }

        public static SD.Image resizeImage(SD.Image imgToResize, SD.Size size)
        {
            int sourceWidth = size.Width; //imgToResize.Width;
            int sourceHeight = size.Height;//imgToResize.Height;

            SD.Bitmap b = new SD.Bitmap(sourceWidth, sourceHeight);
            SD.Graphics g = SD.Graphics.FromImage((SD.Image)b);
            g.InterpolationMode = SDD.InterpolationMode.HighQualityBicubic;
            g.CompositingQuality = SDD.CompositingQuality.HighQuality;

            g.DrawImage(imgToResize, 0, 0, sourceWidth, sourceHeight);
            g.Dispose();

            return (SD.Image)b;
        }
    }

    public enum BlendingMode
    {
        Normal = 0,
        Lighten = 1,
        Darken = 2,
        Multiple = 3,
        Average = 4,
        Add = 5,
        Subtract = 6,
        Difference = 7,
        Negation = 8,
        Screen = 9,
        Exclusive = 10,
        Overlay = 11,
        SoftLight = 12,
        HardLight = 13,
        ColorDodge = 14,
        ColorBurn = 15,
        LinearDodge = 16,
        LinearBurn = 17,
        LinearLight = 18,
        VividLight = 19,
        PinLight = 20,
        HardMix = 21,
        Reflect = 22,
        Glow = 23,
        Phoenix = 24
    }

    public enum MessageBoxType
    {
        Info=0,
        Alert=1,
        Error=2,
        Confirm=3
    }
}
