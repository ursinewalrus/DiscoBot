﻿using System;
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
           // Stopwatch stopWatch1 = new Stopwatch();
            //stopWatch1.Start();
            Gif gif = new Gif(OldGifPath, NewGifPath, oldGif);
            //stopWatch1.Stop();
            //long ts1 = stopWatch1.ElapsedMilliseconds;
            ;

            //could make this a custom object to not have two collections but effect would be the same
            MagickImageCollection collection = new MagickImageCollection();

            var markedFaces = FindFaces(gif);
            FillGaps(markedFaces);

            //casting wrong
            MagickImageCollection sortedCollection = new MagickImageCollection(collection.OrderBy(f => Int32.Parse(f.Comment)));
            string newGifPath = NewGifPath + "giphy_swapped_" + oldGif;
            sortedCollection.Write(newGifPath);
            collection.Dispose();   
            sortedCollection.Dispose();
            return newGifPath;
        }

        private List<List<MarkedFrame>> FindFaces(Gif gif)
        {
            List<List<MarkedFrame>> gifMarkedFrames = new List<List<MarkedFrame>>();
            var frameNum = 0;
            foreach (var frame in gif.Frames)
            {
                Image<Bgra, Byte> originalImage = new Image<Bgra, Byte>(frame.FImagePath);
                Image<Gray, byte> grayframeOriginal = originalImage.Convert<Gray, byte>();
                ConcurrentBag<MarkedFrame> faceDims = new ConcurrentBag<MarkedFrame>();
                Parallel.ForEach(Classifiers, haarFace =>
                {
                    var foundFaces = false;
                    var faces = haarFace.DetectMultiScale(grayframeOriginal, 1.1, 6); //1.01, 10, Size.Empty
                    if (faces.Count() > 0)
                        foundFaces = true;

                    foreach (var face in faces)
                    {
                        var replaceAreas = new MarkedFrame();
                        replaceAreas.FrameNumber = frameNum;
                        replaceAreas.FrameROI = new Rectangle(face.X, face.Y, face.Width, face.Height);
                        replaceAreas.ClassifiedBy = haarFace.ToString();
                        faceDims.Add(replaceAreas);
                        ;
                    }
                    if (!foundFaces)
                    {
                        var placeHolderForNotFound = new MarkedFrame();
                        placeHolderForNotFound.FrameNumber = frameNum;
                        placeHolderForNotFound.FrameROI = new Rectangle(0, 0, 0, 0);
                        placeHolderForNotFound.ClassifiedBy = haarFace.ToString();
                        faceDims.Add(placeHolderForNotFound);
                    }
                });
                List<MarkedFrame> converted = faceDims.ToList();
                //just ensure always end up in the same order for later processing
                converted = converted.OrderBy(cb => cb.ClassifiedBy).ToList();
                gifMarkedFrames.Add(converted);
                frameNum++;
            }
            return gifMarkedFrames;
        }

        private void FillGaps(List<List<MarkedFrame>> markedFrames)
        {
            for (var i = 0; i < Classifiers.Count(); i++)
            {
                //all faces by same classifier, in order of frame
                var facesByCatgorized = markedFrames.Select(f => f[i]).OrderBy(f => f.FrameNumber).ToList();

                var firstFace = facesByCatgorized.Min(f => f.FrameNumber);
                var lastFace = facesByCatgorized.Max(f => f.FrameNumber);

                if (firstFace == lastFace)
                    return;
                //so we know there are gaps now where the classifier found a face, lost it, and picked it up again,
                //we are going to assume it lost it by accident 
                //going to calculate faces x+y velocity and use it to place the face on the missing frames
                float xVel = 0;
                float yVel = 0;
                for(var i=facesByCatgorized.)

            }
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
            //works but due to use case not nesessary atm, user uploaded faces can be assumed to be fine
            //given the current integration
            //Image<Gray, byte> replaceFaceGreyscale = replaceFaceImg.Convert<Gray, byte>();
            //var greyCoordinates = Classifiers[3].DetectMultiScale(replaceFaceGreyscale, 1.01, 10, Size.Empty); //should only be one per
            //if (greyCoordinates.Count() > 0)
            //{
            //    Rectangle roi = new Rectangle(greyCoordinates[0].X, greyCoordinates[0].Y, greyCoordinates[0].Width, greyCoordinates[0].Height);
            //    replaceFaceImg.ROI = roi;
            //    var replaceFace = replaceFaceImg.Copy();
            //    return replaceFace;
            //}
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