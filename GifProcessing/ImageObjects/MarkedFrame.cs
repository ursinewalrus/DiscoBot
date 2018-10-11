using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace GifHolder
{
    class MarkedFrame
    {
        public Rectangle FrameROI { get; set; }
        public int FrameNumber { get; set;  }
        public string ClassifiedBy { get; set; }
        public string Description { get; set; }
    }
}
