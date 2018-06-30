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
            GifProcessing processor = new GifProcessing("ImageProcessing");

            var thing = processor.GifFaceSwap("me.jpg", "giphy.gif");
            ;
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
                    new CascadeClassifier(BasePath + "\\haarcascades\\haarcascade_profileface.xml"),
                    new CascadeClassifier(BasePath + "\\haarcascades\\haarcascade_frontalface_alt_tree.xml")
                };
        }

        //switch or other method for pre found faces
        public string GifFaceSwap(string newFacePath, string gifPath)
        {
            string[] pathParts = gifPath.Split(new string[] { "\\" }, StringSplitOptions.None);
            string fileName = pathParts[pathParts.Length - 1];
            Image<Bgra, Byte> replaceFace = CropReplaceFaces(Classifiers[1], newFacePath);
            string newGif = subInReplaceFace(replaceFace, fileName);
            return newGif;
        }

        private string subInReplaceFace(Image<Bgra, Byte> replaceFace, string oldGif)
        {
            Gif gif = new Gif(OldGifPath, NewGifPath, oldGif);

            int currentFrame = 0;
            MagickImageCollection collection = new MagickImageCollection();

            //foreach (CascadeClassifier classifier in Classifiers)
            //{
            foreach (GifFrame frame in gif.Frames)
            {
                Image<Bgra, Byte> originalImage = new Image<Bgra, Byte>(frame.FImagePath);
                Image<Gray, byte> grayframeOriginal = originalImage.Convert<Gray, byte>();
                var faces = Classifiers[1].DetectMultiScale(grayframeOriginal, 1.01, 10, Size.Empty);
                foreach (var face in faces)
                {
                    var resizedReplaceFace = replaceFace.Resize(face.Width, face.Height, Inter.Linear);
                    Rectangle frameRoi = new Rectangle(face.X, face.Y, face.Width, face.Height);
                    originalImage.ROI = frameRoi;

                    replacePixels(originalImage, resizedReplaceFace);

                    //CvInvoke.cvCopy(resizedReplaceFace, originalImage, IntPtr.Zero);

                    originalImage.ROI = Rectangle.Empty;
                    originalImage.Save(NewGifPath + "edited_giphy_" + oldGif + face.GetHashCode() + ".png");
                    collection.Add(NewGifPath + "edited_giphy_" + oldGif + face.GetHashCode() + ".png");
                    collection[currentFrame].AnimationDelay = frame.FDuration;

                    currentFrame++;
                }
            }
            //}
            string newGifPath = NewGifPath + "giphy_swapped_" + oldGif;
            collection.Write(newGifPath);
            collection.Dispose();
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

                        if (pixelC3 != 0)
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

        private Image<Bgra, byte> CropReplaceFaces(CascadeClassifier classifyer, string newFacePath)
        {
            Image<Bgra, Byte> replaceFaceImg = new Image<Bgra, Byte>(FacesPath + newFacePath);
            Image<Gray, byte> replaceFaceGreyscale = replaceFaceImg.Convert<Gray, byte>();
            var greyCoordinates = classifyer.DetectMultiScale(replaceFaceGreyscale, 1.01, 10, Size.Empty)[0]; //should only be one per
            Rectangle roi = new Rectangle(greyCoordinates.X, greyCoordinates.Y, greyCoordinates.Width, greyCoordinates.Height);
            replaceFaceImg.ROI = roi;
            var replaceFace = replaceFaceImg.Copy();
            return replaceFace;
        }

        public string DownloadGif(string url)
        {
            string dlLocation = "";
            using (var wc = new System.Net.WebClient())
            {
                dlLocation = OldGifPath + DateTimeOffset.UtcNow.ToUnixTimeSeconds() + ".gif";
                wc.DownloadFile(url, dlLocation);
            }
            return dlLocation;
        }
    }
}