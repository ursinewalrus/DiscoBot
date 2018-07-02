using System.Threading.Tasks;
using System;
using Discord.Commands;

namespace Discobot.Utilities
{
    class GifUtilities
    {
        public static async Task DoFaceReplace(ICommandContext Context,string originalGif)
        {
            GifProcessing.GifProcessing processor = new GifProcessing.GifProcessing("ImageManipulation");
            string dlLocation = processor.DownloadGif(originalGif);
            Random random = new Random();
            int facePick = random.Next(0, 2);
            string face = (facePick == 0) ? "kk.png" : "lampreyme.png";
            var newGif = processor.GifFaceSwap(face, dlLocation);
            await Context.Channel.SendFileAsync(newGif);
            return;
            ;
        }
    }
}