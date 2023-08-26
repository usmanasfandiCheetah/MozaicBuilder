using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MosaicUtility.Classes
{
    public class Job
    {
        public string BackImagePath { get; set; }
        public bool BackImageProcessed { get; set; }
        public string OverlayImagePath { get; set; }
        public bool OverlayProcessed { get; set; }
        public string FrameWidth { get; set; }
        public string FrameHeight { get; set; }
        public string StickerWidth { get; set; }
        public string StickerHeight { get; set; }
        public string TotalStickers { get; set; }
        public string BlendValue { get; set; }
        public string Brightness { get; set; }
        public string Contrast { get; set; }
        public string DirWatcherPath { get; set; }
        public string EventCodes { get; set; }
        public int PrintingEnabled { get; set; }
        public string PrinterName { get; set; }
        public string PhotosPerPrint { get; set; }
        public int ImmediatePrint { get; set; }
    }
}
