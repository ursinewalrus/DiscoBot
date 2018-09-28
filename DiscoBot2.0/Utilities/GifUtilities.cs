using System;

namespace Discobot.Utilities
{
    class GifUtilities
    {
        public static string DoFaceReplace(string originalGif, string face)
        {
            GifProcessing.GifProcessing processor = new GifProcessing.GifProcessing("ImageManipulation");
            string dlLocation = processor.DownloadGif(originalGif);
            var newGif = processor.GifFaceSwap(face, dlLocation);
            return newGif;
        }
    }
}