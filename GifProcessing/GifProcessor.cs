using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GifHolder;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.Util;
using Emgu.CV.CvEnum;
using System.Drawing;
using ImageMagick;
using System.Text.RegularExpressions;
using System.Collections.Concurrent;
using System.Diagnostics;
//http://alereimondo.no-ip.org/OpenCV/34 - more cascades
namespace GifProcessing
{
    public class GifProcessing
    {
        public string ImagesFolder;
        public string BasePath; 
        public string FacesPath;
        public string OldGifPath; 
        public string NewGifPath;

        public int NthFrameToAnalyze;

        public List<CascadeClassifier> Classifiers;

        static void Main(string[] args){}

        public GifProcessing(string imagesFolder)
        {
            this.ImagesFolder = imagesFolder;
            this.BasePath = Path.GetDirectoryName(Path.GetDirectoryName(Directory.GetCurrentDirectory())) + "\\" + this.ImagesFolder;
            this.FacesPath = BasePath + "\\replacementFaces\\";
            this.OldGifPath = BasePath + "\\TempGifs\\";
            this.NewGifPath = BasePath + "\\EditedGifs\\";
            this.Classifiers = new List<CascadeClassifier>()
                {
                    new CascadeClassifier(BasePath + "\\haarcascades\\haarcascade_frontalface_alt_tree.xml"),
                    new CascadeClassifier(BasePath + "\\haarcascades\\haarcascade_frontalcatface.xml"),
                    new CascadeClassifier(BasePath + "\\haarcascades\\haarcascade_frontalcatface_extended.xml"),
                    new CascadeClassifier(BasePath + "\\haarcascades\\haarcascade_frontalface_alt.xml"),
                    new CascadeClassifier(BasePath + "\\haarcascades\\haarcascade_frontalface_alt2.xml"),
                    new CascadeClassifier(BasePath + "\\haarcascades\\haarcascade_frontalface_default.xml"),
                };
        }

        //switch or other method for pre found faces
        public string GifFaceSwap(List<String> newFacePaths, string gifPath)
        {
            string[] pathParts = gifPath.Split(new string[] { "\\" }, StringSplitOptions.None);
            string fileName = pathParts[pathParts.Length - 1];
            //...if gif gather the indev images, pass them in, CropReplaceFaces should take List<Image<Bgra, Byte>>
            List<Image<Bgra, Byte>>replaceFace  = new List<Image<Bgra, Byte>>();
            newFacePaths.ForEach(f => replaceFace.Add(new Image<Bgra, Byte>(FacesPath + f)));
            string newGif = subInReplaceFace(replaceFace, fileName);
            return newGif;
        }

        private string subInReplaceFace(List<Image<Bgra, Byte>> replaceFace, string oldGif)
        {
            #region init_stuff
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            Gif gif = new Gif(OldGifPath, NewGifPath, oldGif);
            stopWatch.Stop();
            long ts = stopWatch.ElapsedMilliseconds;
            Console.WriteLine("Gif creation took:" + ts.ToString() + " ms");
            ;
            ConcurrentBag<int> totalFacesReplaced = new ConcurrentBag<int>();
            ConcurrentBag<int> framesWithFacesReplaced = new ConcurrentBag<int>();

            //could make this a custom object to not have two collections but effect would be the same
            MagickImageCollection collection = new MagickImageCollection();


            Stopwatch stopWatch1 = new Stopwatch();
            stopWatch1.Start();

            #endregion

            #region speed_optimizer_setup
            //cordinates of rectangles in frames by index we think should have my face over them
            //as determined by them being sandwiched between frames with confirmed faces
            //and rectangles as determined by the average coordinates of the sandwiching frame
            //rectangles
            List<Rectangle>[] windowEdgeFrames = new List<Rectangle>[gif.Frames.Count()];
            #endregion

            #region the_work

            //int currentFrame = 0;
            //foreach (var frame in gif.Frames)
            //Parallel.ForEach(gif.Frames, frame =>
            for (var currentFrame = 0; currentFrame < gif.Frames.Count(); currentFrame++)
            {

                if(currentFrame % 2 == 0)
                {
                    continue;
                }
                var frame = gif.Frames[currentFrame];
                //skip every other frame, TODO make this an arg cripes
               

                // Stopwatch frameWatch = new Stopwatch();
                // frameWatch.Start();
                Image<Bgra, Byte> originalImage = new Image<Bgra, Byte>(frame.FImagePath);
                Image<Gray, byte> grayframeOriginal = originalImage.Convert<Gray, byte>();

                ConcurrentBag<Tuple<Rectangle, Image<Bgra, byte>>> faceDims = LocateFaces(replaceFace, currentFrame, grayframeOriginal);
                
                foreach (var faceROI in faceDims)
                {
                    if(windowEdgeFrames[currentFrame] == null)
                    {
                        windowEdgeFrames[currentFrame] = new List<Rectangle>();
                    }
                    windowEdgeFrames[currentFrame].Add(faceROI.Item1);
                    framesWithFacesReplaced.Add(1);
                    originalImage.ROI = faceROI.Item1;
                    replacePixels(originalImage, faceROI.Item2);
                }

                //we saw a face earlier, now we see one again
               
                //Parallel.ForEach(Classifiers, haarFace =>{});

                //CvInvoke.cvCopy(resizedReplaceFace, originalImage, IntPtr.Zero);
                originalImage.ROI = Rectangle.Empty;
                //probs just give it another name for if edited or not
                originalImage.Save(NewGifPath + "edited_giphy_" + currentFrame + "_" + oldGif + frame.GetHashCode() + ".png");


                collection.Add(NewGifPath + "edited_giphy_" + currentFrame + "_" + oldGif + frame.GetHashCode() + ".png");

                // File.Delete(NewGifPath + "edited_giphy_" + oldGif + frame.GetHashCode() + ".png");

                collection.Last().AnimationDelay = frame.FDuration;
                collection.Last().Comment = frame.Frame.ToString();

                // frameWatch.Stop();
                //  long frameTime = frameWatch.ElapsedMilliseconds;

                //Console.WriteLine("frame "+ currentFrame.ToString()+" took:" + frameTime.ToString() + " ms to process");
                //currentFrame++;

            };
            #endregion

            #region writing_&_analytics
            Console.WriteLine(framesWithFacesReplaced.Count().ToString() + " with faces replaced");
            Console.WriteLine(totalFacesReplaced.Count().ToString() + " total facecs replaced");
            ;
            //casting wrong
            MagickImageCollection sortedCollection = new MagickImageCollection(collection.OrderBy(f => Int32.Parse(f.Comment)));
            stopWatch1.Stop();
            long ts1 = stopWatch1.ElapsedMilliseconds;

            Console.WriteLine("face subbing took:" + ts1.ToString() + " ms for " + gif.Frames.Count().ToString() + " frames");

            Stopwatch stopWatch2 = new Stopwatch();
            stopWatch2.Start();
            string newGifPath = NewGifPath + "giphy_swapped_" + oldGif;

            PreGifAnalytics(sortedCollection);

            QuantizeSettings settings = new QuantizeSettings();
            settings.Colors = 8;
            sortedCollection.Quantize(settings);

            sortedCollection.Optimize();
            sortedCollection.Write(newGifPath);

            collection.Dispose();
            sortedCollection.Dispose();
            stopWatch2.Stop();
            long ts2 = stopWatch2.ElapsedMilliseconds;
            Console.WriteLine("The rest took:" + ts2.ToString() + " ms");
            #endregion
            var a = windowEdgeFrames;
            var l = windowEdgeFrames.Length;
            return newGifPath;
        }

        private ConcurrentBag<Tuple<Rectangle, Image<Bgra, byte>>> LocateFaces(List<Image<Bgra, byte>> replaceFace, int currentFrame, Image<Gray, byte> grayframeOriginal)
        {
            ConcurrentBag<Tuple<Rectangle, Image<Bgra, Byte>>> faceDims = new ConcurrentBag<Tuple<Rectangle, Image<Bgra, Byte>>>();

            //test for speed at some point vs just a for loop
            Parallel.ForEach(Classifiers, haarFace =>
            {
                //set ROI on past ones to speed up
                //grayframeOriginal.ROI = new Rectangle(0, 0, 10, 10);
                var faces = haarFace.DetectMultiScale(grayframeOriginal, 1.1, 6); //1.01, 10, Size.Empty

                foreach (var face in faces)
                {
                    var resizedReplaceFace = replaceFace[(currentFrame % (2 * replaceFace.Count())) / 2].Resize(face.Width, face.Height, Inter.Linear);
                    Rectangle frameRoi = new Rectangle(face.X, face.Y, face.Width, face.Height);
                    faceDims.Add(Tuple.Create(frameRoi, resizedReplaceFace));
                    //originalImage.ROI = frameRoi;

                    //replacePixels(originalImage, resizedReplaceFace);
                }

            });
            return faceDims;
        }

        private static void replacePixels(Image<Bgra, byte> originalImage, Image<Bgra, byte> resizedReplaceFace)
        {
            for (int w = 0; w < originalImage.ROI.Width; w++)
            {
                for (int h = 0; h < originalImage.ROI.Height; h++)
                {
                    for (int p = 0; p < 4; p++)
                    {
                        byte pixelC0 = resizedReplaceFace.Data[h, w, 0];
                        byte pixelC1 = resizedReplaceFace.Data[h, w, 1];
                        byte pixelC2 = resizedReplaceFace.Data[h, w, 2];
                        byte pixelC3 = resizedReplaceFace.Data[h, w, 3];

                        if (pixelC3 != 0 && !(pixelC0 == 255 && pixelC1 == 255 && pixelC2 == 255) )
                        {
                            originalImage.Data[originalImage.ROI.Top + h, originalImage.ROI.Left + w, 0] = pixelC0;
                            originalImage.Data[originalImage.ROI.Top + h, originalImage.ROI.Left + w, 1] = pixelC1;
                            originalImage.Data[originalImage.ROI.Top + h, originalImage.ROI.Left + w, 2] = pixelC2;
                            originalImage.Data[originalImage.ROI.Top + h, originalImage.ROI.Left + w, 3] = pixelC3;

                        }
                    }
                }
            }
        }


        private void PreGifAnalytics(MagickImageCollection imgCollection)
        {
            int totalPixels = 0;
            int totalFrames = 0;
            foreach(var img in imgCollection)
            {
                totalPixels += img.Height * img.Width;
                totalFrames++;
            }
            Console.WriteLine("Analytics: Total frames is" + totalFrames);
            Console.WriteLine("Analytics: Total pixels is " + totalPixels);
            Console.WriteLine("Analytics: Avg pixels is " + totalPixels / totalFrames);
            Console.WriteLine("Analytics: Img Height is " + imgCollection[0].Height);
            Console.WriteLine("Analytics: Img Width is " + imgCollection[0].Width);



        }

        public string DownloadGif(string url)
        {
            string dlLocation = "";
            using (var wc = new System.Net.WebClient())
            {
                dlLocation = OldGifPath + DateTimeOffset.UtcNow.ToUnixTimeSeconds() + (Path.GetExtension(url) ?? ".gif");
                wc.DownloadFile(url, dlLocation);
            }
            return dlLocation;
        }
    }
}