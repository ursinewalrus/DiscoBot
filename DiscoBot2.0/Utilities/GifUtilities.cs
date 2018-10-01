using System;
using System.IO;

namespace Discobot.Utilities
{
    class GifUtilities
    {
        public static string DefaultFace = "lampreyme.png";
        public static string DoFaceReplace(string originalGif, string face)
        {
            GifProcessing.GifProcessing processor = new GifProcessing.GifProcessing("ImageManipulation");
            string dlLocation = processor.DownloadGif(originalGif);
            if (!File.Exists(processor.FacesPath + face))
            {
                face = DefaultFace;
            }
            var newGif = processor.GifFaceSwap(face, dlLocation);
            return newGif;
        }
    }
}