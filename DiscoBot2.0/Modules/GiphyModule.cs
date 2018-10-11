using System;
using Discord.Commands;
using System.Threading.Tasks;
using Discobot.Utilities;
using System.Configuration;
using System.Net;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Linq;

namespace Discobot.Modules
{

    [Name("Giphy")]
    public class GiphyModule : ModuleBase
    {
        private string GiphyKey => ConfigurationManager.AppSettings["giphykey"];

        //drop in a command to do the try catch or not
        [Command("jiffy")]
        public async Task GiphyGet([Remainder] string input = "")
        {
            var faceArgs = input.Split(' ');
            var face = GifUtilities.DefaultFace;

            if (faceArgs[0].ToLower().Contains("face:") && faceArgs[0].Split(':').Length == 2){
                face = faceArgs[0].Split(':')[1];
                input = String.Join(" ", faceArgs.Skip(1));
            }

            string searchString = "http://api.giphy.com/v1/gifs/translate?api_key=" + GiphyKey + "&s=" + Uri.EscapeDataString(input);
            var json = new WebClient().DownloadString(searchString);
            JToken giffyToken = JObject.Parse(json);
            string url = giffyToken.SelectToken("data").SelectToken("url").ToString(); 
            string originalGif = giffyToken.SelectToken("data").SelectToken("images").SelectToken("original").SelectToken("url").ToString();
            try
            {
                string gif = GifUtilities.DoFaceReplace(originalGif, face);
                await Context.Channel.SendFileAsync(gif);
                return;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            await ReplyAsync(url);
        }

        [Command("jiffyFaces")]
        public async Task GiphyGet()
        {
            GifProcessing.GifProcessing processor = new GifProcessing.GifProcessing("ImageManipulation");
            var faceFiles = String.Join("\n", Directory.GetFiles(processor.FacesPath).Select(f => f.Split('\\').Last() ));
            await ReplyAsync(faceFiles);
        }

    }
}
