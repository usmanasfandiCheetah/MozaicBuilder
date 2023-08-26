using AForge.Imaging.Filters;
using MyEffects;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;

namespace MosaicUtility.Classes
{
    public class MosaicHelper
    {
        public string SourceFolder { get; set; }
        public string DestFolder { get; set; }
        public string FilterFolder { get; set; }
        public string OverlayFolder { get; set; }
        private Random rnd = null;
        public BlendingMode BlendName { get; set; }
        public float BlendValue { get; set; }
        public float Contrast { get; set; }
        public float Brightness { get; set; }
        public bool NoMoreSlices { get; set; }

        public MosaicHelper(string mainDirectory)
        {
            rnd = new Random();
            SourceFolder = mainDirectory + "sources\\";
            DestFolder = mainDirectory + "processed\\";
            FilterFolder = mainDirectory + "filtered\\";
            OverlayFolder = mainDirectory + "overlays\\";

            if (!Directory.Exists(DestFolder)) Directory.CreateDirectory(DestFolder);
            if (!Directory.Exists(FilterFolder)) Directory.CreateDirectory(FilterFolder);

            BlendValue = 0.35f;
            BlendName = BlendingMode.Normal;
            Contrast = 1f;
            Brightness = 1f;
        }


        #region Blend Processor
        public string ProcessRandom(string sourcePhoto, Size output)
        {
            try
            {
                if (Directory.Exists(SourceFolder))
                {
                    var files = Directory.GetFiles(SourceFolder).ToList();
                    if (files.Count > 0)
                    {
                        int currentIndex = rnd.Next(files.Count);
                        string backFile = files[currentIndex];
                        var res = ProcessSingle(backFile, sourcePhoto, output);
                        return res;
                    }
                    else
                        NoMoreSlices = true;

                    return "";
                }
                return "";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
        public bool ProcessAll()
        {
            try
            {
                if (Directory.Exists(SourceFolder))
                {

                    var files = Directory.GetFiles(SourceFolder).ToList();
                    int currentIndex = rnd.Next(files.Count);
                    for (int i = 0; i < files.Count; i++)
                    {

                    }
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        public string ProcessSingle(string backFile, string userPhoto, Size outputSize)
        {
            try
            {
                var filename = Path.GetFileName(backFile);

                var refined = filename.Replace("C", "").Replace("R", "").Replace(".jpg", "").Replace(".png", "");
                var row = Convert.ToInt32(refined.Split('_').First().Trim()) - 1;
                var col = Convert.ToInt32(refined.Split('_').Last().Trim()) - 1;

                var im1 = Image.FromFile(userPhoto);
                var im2 = Image.FromFile(backFile);

                int width = im2.Width, height = im2.Height;
                int X = row * width, Y = col * height;

                var source = (Bitmap)GetResizedCoppedImage(im1, outputSize);        // user photo
                var cloned = (Bitmap)GetResizedCoppedImage(im2, outputSize);        

                im1.Dispose();
                im2.Dispose();


                Bitmap Overlayed = null;
                string overlayPath = OverlayFolder + filename.Replace(".jpg", ".png");
                if (File.Exists(overlayPath))
                {
                    Overlayed = new Bitmap(overlayPath);
                }


                GaussianBlur blur = new GaussianBlur(3, 5);
                blur.ApplyInPlace(cloned);


                //var layer2 = Blend(trans, cloned);
                // extra for this event
                using (Graphics ggg = Graphics.FromImage(cloned))
                {
                    var trans = SetImageOpacity(source, BlendValue, Contrast, Brightness);
                    ggg.DrawImage(trans, new Rectangle(0, 0, cloned.Width, cloned.Height));
                    ggg.Dispose();
                    trans.Dispose();
                }
                if (Overlayed != null)
                {
                    using (Graphics g = Graphics.FromImage(cloned))
                    {
                        g.DrawImage(Overlayed, new Rectangle(0, 0, cloned.Width, cloned.Height));
                        g.Dispose();
                    }

                    Overlayed.Dispose();
                }
                var filteredPath = FilterFolder + "R" + (row + 1) + "_C" + (col + 1) + ".png";
                SaveImage(row, col, cloned, filteredPath);
                //layer2.Dispose();
                source.Dispose();
                cloned.Dispose();

                File.Move(backFile, DestFolder + filename);

                return filteredPath;
            }
            catch (Exception ex)
            {
                Globals.WriteErrorLog(ex.Message);
                return ex.Message;
            }
        }

        Bitmap Blend(Bitmap s, Bitmap c)
        {
            Bitmap layer2 = null;
            switch (BlendName)
            {
                case BlendingMode.Add:
                    layer2 = new BlendFilter(BlendMode.Add, c).Apply(s);
                    break;
                case BlendingMode.Average:
                    layer2 = new BlendFilter(BlendMode.Average, c).Apply(s);
                    break;
                case BlendingMode.ColorBurn:
                    layer2 = new BlendFilter(BlendMode.ColorBurn, c).Apply(s);
                    break;
                case BlendingMode.ColorDodge:
                    layer2 = new BlendFilter(BlendMode.ColorDodge, c).Apply(s);
                    break;
                case BlendingMode.Darken:
                    layer2 = new BlendFilter(BlendMode.Darken, c).Apply(s);
                    break;
                case BlendingMode.Difference:
                    layer2 = new BlendFilter(BlendMode.Difference, c).Apply(s);
                    break;
                case BlendingMode.Exclusive:
                    layer2 = new BlendFilter(BlendMode.Exclusion, c).Apply(s);
                    break;
                case BlendingMode.Glow:
                    layer2 = new BlendFilter(BlendMode.Glow, c).Apply(s);
                    break;
                case BlendingMode.HardLight:
                    layer2 = new BlendFilter(BlendMode.HardLight, c).Apply(s);
                    break;
                case BlendingMode.HardMix:
                    layer2 = new BlendFilter(BlendMode.HardMix, c).Apply(s);
                    break;
                case BlendingMode.Lighten:
                    layer2 = new BlendFilter(BlendMode.Lighten, c).Apply(s);
                    break;
                case BlendingMode.LinearBurn:
                    layer2 = new BlendFilter(BlendMode.LinearBurn, c).Apply(s);
                    break;
                case BlendingMode.LinearDodge:
                    layer2 = new BlendFilter(BlendMode.LinearDodge, c).Apply(s);
                    break;
                case BlendingMode.LinearLight:
                    layer2 = new BlendFilter(BlendMode.LinearLight, c).Apply(s);
                    break;
                case BlendingMode.Multiple:
                    layer2 = new BlendFilter(BlendMode.Multiply, c).Apply(s);
                    break;
                case BlendingMode.Negation:
                    layer2 = new BlendFilter(BlendMode.Negation, c).Apply(s);
                    break;
                case BlendingMode.Normal:
                    layer2 = new BlendFilter(BlendMode.Normal, c).Apply(s);
                    break;
                case BlendingMode.Overlay:
                    layer2 = new BlendFilter(BlendMode.Overlay, c).Apply(s);
                    break;

            }

            return layer2;
        }

        public Bitmap SetImageOpacity(Image image, float opacity, float contrast, float brightness)
        {
            try
            {
                float adjustedBrightness = brightness - 1.0f;
                //create a Bitmap the size of the image provided  
                Bitmap bmp = new Bitmap(image.Width, image.Height);

                //create a graphics object from the image  
                using (Graphics gfx = Graphics.FromImage(bmp))
                {


                    // create matrix that will brighten and contrast the image
                    float[][] ptsArray ={
                    new float[] {contrast, 0, 0, 0, 0}, // scale red
                    new float[] {0, contrast, 0, 0, 0}, // scale green
                    new float[] {0, 0, contrast, 0, 0}, // scale blue
                    new float[] {0, 0, 0, opacity, 0}, // don't scale alpha
                    new float[] {adjustedBrightness, adjustedBrightness, adjustedBrightness, 0, 1}};
                    //set the opacity  
                    //create a color matrix object  
                    ColorMatrix matrix = new ColorMatrix(ptsArray);
                    // matrix.Matrix33 = opacity;

                    //create image attributes  
                    ImageAttributes attributes = new ImageAttributes();

                    //set the color(opacity) of the image  
                    attributes.SetColorMatrix(matrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);

                    //now draw the image  
                    gfx.DrawImage(image, new Rectangle(0, 0, bmp.Width, bmp.Height), 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, attributes);
                }
                return bmp;
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message);
                return null;
            }

        }
        private void SaveImage(int row, int col, Bitmap bt, string path)
        {
            try
            {

                var ly = (Bitmap)bt.Clone();
                ly.SetResolution(300, 300);
                using (Graphics g = Graphics.FromImage(ly))
                {
                    g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                    g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                    g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
                    LinearGradientBrush linGrBrush = new LinearGradientBrush(
                                                                               new Point(0, 10),
                                                                               new Point(200, 10),
                                                                               Color.FromArgb(255, 64, 64, 64),
                                                                               Color.FromArgb(150, 64, 129, 64));
                    g.DrawString("R" + (row + 1) + " C" + (col + 1), new Font("Calibri", 8, FontStyle.Bold), linGrBrush, new PointF(5, 6));
                    g.DrawString("R" + (row + 1) + " C" + (col + 1), new Font("Calibri", 8), Brushes.White, new PointF(5, 5));
                    g.Dispose();
                }
                ly.Save(path, ImageFormat.Png);
                ly.Dispose();
            }
            catch (Exception ex)
            {

            }
        }
        public Image GetResizedCoppedImage(Image source, Size target)
        {
            Image targetImage = null;

            try
            {
                float RatioH = target.Height / (float)source.Height;
                float RatioW = target.Width / (float)source.Width;
                if (RatioH > RatioW)
                {
                    int newWidth = (int)Math.Round(source.Width * RatioH);
                    int newHeight = target.Height;
                    int X = (newWidth - target.Width) / 2, Y = 0;
                    Bitmap b = new Bitmap(source, new Size(newWidth, newHeight));
                    b.SetResolution(300, 300);
                    targetImage = b.Clone(new RectangleF(X, Y, target.Width, target.Height), PixelFormat.Format32bppArgb);
                    b.Dispose();
                }
                else
                {
                    int newWidth = target.Width;
                    int newHeight = (int)Math.Round(source.Height * RatioW);
                    int Y = (newHeight - target.Height) / 2, X = 0;
                    Bitmap b = new Bitmap(source, new Size(newWidth, newHeight));
                    b.SetResolution(300, 300);
                    targetImage = b.Clone(new RectangleF(X, Y, target.Width, target.Height), PixelFormat.Format32bppArgb);
                    b.Dispose();
                }

            }
            catch (Exception ex)
            {
            }

            return targetImage;
        }
        #endregion

        #region Color Management
        public float[] GetAvgColor(Bitmap bm)
        {
            BitmapData srcData = bm.LockBits(
            new Rectangle(0, 0, bm.Width, bm.Height),
            ImageLockMode.ReadOnly,
            PixelFormat.Format32bppArgb);

            int stride = srcData.Stride;

            IntPtr Scan0 = srcData.Scan0;

            long[] totals = new long[] { 0, 0, 0 };

            int width = bm.Width;
            int height = bm.Height;

            unsafe
            {
                byte* p = (byte*)(void*)Scan0;

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        for (int color = 0; color < 3; color++)
                        {
                            int idx = (y * stride) + x * 4 + color;

                            totals[color] += p[idx];
                        }
                    }
                }
            }
            var bgr = new float[3];
            bgr[0] = totals[0] / (width * height);
            bgr[1] = totals[1] / (width * height);
            bgr[2] = totals[2] / (width * height);
            bm.UnlockBits(srcData);
            return bgr;
        }
        public int GetDistance(Color current, Color match)
        {
            int redDifference;
            int greenDifference;
            int blueDifference;
            int alphaDifference;

            alphaDifference = current.A - match.A;
            redDifference = current.R - match.R;
            greenDifference = current.G - match.G;
            blueDifference = current.B - match.B;

            return alphaDifference * alphaDifference + redDifference * redDifference +
                                     greenDifference * greenDifference + blueDifference * blueDifference;
        }
        public ColorData FindNearestColor(List<ColorData> map, Color current)
        {
            int shortestDistance;
            ColorData nearestColor = new ColorData();

            shortestDistance = int.MaxValue;

            for (int i = 0; i < map.Count; i++)
            {
                Color match;
                int distance;

                match = ColorTranslator.FromHtml(map[i].C);
                distance = GetDistance(current, match);

                if (distance < shortestDistance)
                {
                    nearestColor = map[i];
                    shortestDistance = distance;
                }
            }

            return nearestColor;
        }
        #endregion
    }
    public class ColorData
    {
        public int X { get; set; }
        public int Y { get; set; }
        public string C { get; set; }
        public int I { get; set; }
        public int S { get; set; }
    }
}
