using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace GifHolder
{
    public class Gif
    {
        public List<GifFrame> Frames = new List<GifFrame>();
        public Gif(string path, string newPath, string file)
        {
            Image img;
            if (Path.GetExtension(file) != ".gif")
            {
                var convertedImg = new Bitmap(path + file);
                convertedImg.Save(path+ file + ".gif", ImageFormat.Gif);
            }
            img = Image.FromFile(path + file);
            //try these if cant not gif just skip
            int frameNums;
            byte[] times;
            try
            {
                frameNums = img.GetFrameCount(FrameDimension.Time);
                times = img.GetPropertyItem(0x5100).Value;
            }
            catch
            {
                frameNums = 1;
                times = new byte[]{1,1,1,1};
            }
            for (int i = 0; i < frameNums; i++)
            {
                int duration = BitConverter.ToInt32(times, 4 * i);
                Frames.Add(new GifFrame(duration, new Bitmap(img), file, newPath, i));
                img.SelectActiveFrame(FrameDimension.Time, i);
            }
            img.Dispose();
        }

    }

    public class GifFrame
    {
        public int FDuration { get; set; }
        public Image FImage { get; set; }
        public string FImagePath { get; set; }
        public int Frame { get; set; }
        //make images and save paths here for it
        public GifFrame(int dur, Image image, string name, string newPath, int frame)
        {
            this.FDuration = dur;
            this.FImage = image;
            this.Frame = frame;
            this.FImagePath = newPath + name + "_"+ frame + ".png";
            FImage.Save(FImagePath, ImageFormat.Png);
            ;
        }
    }
}
