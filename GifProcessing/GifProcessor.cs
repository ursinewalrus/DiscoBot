using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GifHolder;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.CvEnum;
using System.Drawing;
using ImageMagick;
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

        public List<CascadeClassifier> Classifiers;

        static void Main(string[] args)
        {
            //loop for FEATURES????
            //GifProcessing processor = new GifProcessing("ImageProcessing");

            //  var thing = processor.GifFaceSwap("me.jpg", "giphy.gif");
            //  ;
            // string download = processor.DownloadGif("https://media3.giphy.com/media/ypqHf6pQ5kQEg/giphy.gif");


        }

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
        public string GifFaceSwap(string newFacePath, string gifPath)
        {
            string[] pathParts = gifPath.Split(new string[] { "\\" }, StringSplitOptions.None);
            string fileName = pathParts[pathParts.Length - 1];
            Image<Bgra, Byte> replaceFace = CropReplaceFaces(newFacePath);
            string newGif = subInReplaceFace(replaceFace, fileName);
            return newGif;
        }

        private string subInReplaceFace(Image<Bgra, Byte> replaceFace, string oldGif)
        {
            Stopwatch stopWatch1 = new Stopwatch();
            stopWatch1.Start();
            Gif gif = new Gif(OldGifPath, NewGifPath, oldGif);
            stopWatch1.Stop();
            long ts1 = stopWatch1.ElapsedMilliseconds;
            ;

            int currentFrame = 0;
            //could make this a custom object to not have two collections but effect would be the same
            MagickImageCollection collection = new MagickImageCollection();


            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            foreach(var frame in gif.Frames)
            //foreach (GifFrame frame in gif.Frames)
            {
                Image<Bgra, Byte> originalImage = new Image<Bgra, Byte>(frame.FImagePath);
                Image<Gray, byte> grayframeOriginal = originalImage.Convert<Gray, byte>();

                ConcurrentBag<Tuple<Rectangle,Image<Bgra,Byte>>> faceDims = new ConcurrentBag<Tuple<Rectangle, Image<Bgra, Byte>>>();

                //test for speed at some point vs just a for loop
                Parallel.ForEach(Classifiers, haarFace =>
                {
                    var faces = haarFace.DetectMultiScale(grayframeOriginal,1.1,6); //1.01, 10, Size.Empty

                    foreach (var face in faces)
                    {
                        var resizedReplaceFace = replaceFace.Resize(face.Width, face.Height, Inter.Linear);
                        Rectangle frameRoi = new Rectangle(face.X, face.Y, face.Width, face.Height);
                        faceDims.Add(Tuple.Create(frameRoi, resizedReplaceFace));
                        //originalImage.ROI = frameRoi;

                        //replacePixels(originalImage, resizedReplaceFace);
                    }

                });
                foreach(var faceROI in faceDims)
                {
                    originalImage.ROI = faceROI.Item1;

                    replacePixels(originalImage, faceROI.Item2);
                }
                //Parallel.ForEach(Classifiers, haarFace =>{});

                //CvInvoke.cvCopy(resizedReplaceFace, originalImage, IntPtr.Zero);
                originalImage.ROI = Rectangle.Empty;
                originalImage.Save(NewGifPath + "edited_giphy_" + oldGif + frame.GetHashCode() + ".png");
                collection.Add(NewGifPath + "edited_giphy_" + oldGif + frame.GetHashCode() + ".png");
                File.Delete(NewGifPath + "edited_giphy_" + oldGif + frame.GetHashCode() + ".png");
                collection[currentFrame].AnimationDelay = frame.FDuration;
                collection[currentFrame].Comment = frame.Frame.ToString();

                currentFrame++;
            };
            ;
            //casting wrong
            MagickImageCollection sortedCollection = new MagickImageCollection(collection.OrderBy(f => Int32.Parse(f.Comment)));
            stopWatch.Stop();
            long ts = stopWatch.ElapsedMilliseconds;
            ;
            string newGifPath = NewGifPath + "giphy_swapped_" + oldGif;
            sortedCollection.Write(newGifPath);
            collection.Dispose();
            sortedCollection.Dispose();
            return newGifPath;
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

        private Image<Bgra, byte> CropReplaceFaces(string newFacePath)
        {
            Image<Bgra, Byte> replaceFaceImg = new Image<Bgra, Byte>(FacesPath + newFacePath);
            Image<Gray, byte> replaceFaceGreyscale = replaceFaceImg.Convert<Gray, byte>();
            var greyCoordinates = Classifiers[3].DetectMultiScale(replaceFaceGreyscale, 1.01, 10, Size.Empty); //should only be one per
            if (greyCoordinates.Count() > 0)
            {
                Rectangle roi = new Rectangle(greyCoordinates[0].X, greyCoordinates[0].Y, greyCoordinates[0].Width, greyCoordinates[0].Height);
                replaceFaceImg.ROI = roi;
                var replaceFace = replaceFaceImg.Copy();
                return replaceFace;
            }
                return replaceFaceImg; 
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