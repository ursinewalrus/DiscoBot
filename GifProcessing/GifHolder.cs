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
            Image img = Image.FromFile(path + file);
            int frameNums = img.GetFrameCount(FrameDimension.Time);
            byte[] times = img.GetPropertyItem(0x5100).Value;

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

        //make images and save paths here for it
        public GifFrame(int dur, Image image, string name, string newPath, int frame)
        {
            FDuration = dur;
            FImage = image;
            FImagePath = newPath + name + "_"+ frame + ".png";
            FImage.Save(FImagePath, ImageFormat.Png);
            ;
        }
    }
}
