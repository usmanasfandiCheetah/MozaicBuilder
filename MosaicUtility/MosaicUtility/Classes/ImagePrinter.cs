using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Printing;
using System.Runtime.InteropServices;

namespace MosaicUtility.Classes
{
    public class ImagePrinter
    {
        public int width { get; set; }
        public int height { get; set; }

        #region constructor

        public ImagePrinter()
        {
        }

        #endregion constructor

        #region delegates

        private static bool ThumbnailAbort()
        {
            return false;
        }

        private Image.GetThumbnailImageAbort thumbnail_callback =
            new Image.GetThumbnailImageAbort(ThumbnailAbort);

        private void Print_Handler(object sender, PrintPageEventArgs e)
        {
            //Image img = _imagefile;
            //e.PageSettings.Landscape = true;
            //width = e.PageBounds.Width;// Convert.ToInt16((img.Width / img.HorizontalResolution) * 100);
            //height = e.PageBounds.Height;// Convert.ToInt16((img.Height / img.VerticalResolution) * 100);
            e.PageSettings.Margins = new Margins(0, 0, 0, 0);

            Bitmap img = (Bitmap)_imagefile;
            img.SetResolution(360, 360);
            e.Graphics.DrawImage(img, new Rectangle(0, 0, width, height));

            //e.Graphics.DrawImage(img, new Rectangle(0, 0, width, height), new Rectangle(0, 0, img.Width, img.Height), GraphicsUnit.Pixel);

            //Image img = _imagefile;
            //if (img.Width > img.Height)
            //{
            //    //PageSettings.Landscape = true;
            //    int width = e.PageBounds.Width;// Convert.ToInt16((img.Width / img.HorizontalResolution) * 100);
            //    int height = e.PageBounds.Height;// Convert.ToInt16((img.Height / img.VerticalResolution) * 100);
            //    e.PageSettings.Margins = new Margins(0, 0, 0, 0);
            //    //--------------
            //    float ratio = ((float)width / img.Width);
            //    int tempHeight = Convert.ToInt32(img.Height * ratio);
            //    //----------------------
            //    int y = Math.Abs(height - tempHeight) / 2;
            //    e.Graphics.DrawImage(img, new Rectangle(0, y, width, tempHeight), new Rectangle(0, 0, img.Width, img.Height), GraphicsUnit.Pixel);
            //}
            //else
            //{
            //    //PageSettings.Landscape = true;
            //    int width = e.PageBounds.Width;// Convert.ToInt16((img.Width / img.HorizontalResolution) * 100);
            //    int height = e.PageBounds.Height;// Convert.ToInt16((img.Height / img.VerticalResolution) * 100);
            //    e.PageSettings.Margins = new Margins(0, 0, 0, 0);
            //    //--------------
            //    float ratio = ((float)height / img.Height);
            //    int tempWidth = Convert.ToInt32(img.Width * ratio);
            //    //----------------------
            //    int y = Math.Abs(width - tempWidth) / 2;
            //    e.Graphics.DrawImage(img, new Rectangle(y, 0, tempWidth, height), new Rectangle(0, 0, img.Width, img.Height), GraphicsUnit.Pixel);
            //}
        }

        #endregion delegates

        #region private

        private Image _imagefile;
        private Image _frame;

        #endregion private

        #region public

        public void PrintImage(Image imagefilename, Image frame)
        {
            _imagefile = imagefilename;
            _frame = frame;
            PrintDocument printer = new PrintDocument();

            //printer.DefaultPageSettings.Landscape = true;

            printer.PrintPage +=
                new PrintPageEventHandler(Print_Handler);

            printer.PrinterSettings.Copies = 1;

            printer.Print();
        }

        #endregion public
    }

    public static class PrinterClass
    {
        [DllImport("winspool.drv", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool SetDefaultPrinter(string Printer);
    }
}
