using System;
using Discord.Commands;
using System.Threading.Tasks;
using Discobot.Utilities;
using System.Configuration;
using System.Net;
using Newtonsoft.Json.Linq;

namespace Discobot.Modules
{

    [Name("Giphy")]
    public class GiphyModule : ModuleBase
    {
        private string GiphyKey => ConfigurationManager.AppSettings["giphykey"];

        //drop in a command to do the try catch or not
        [Command("jiffy")]
        public async Task GiphyGet([Remainder] string input)
        {
            string searchString = "http://api.giphy.com/v1/gifs/translate?api_key=" + GiphyKey + "&s=" + Uri.EscapeDataString(input);
            var json = new WebClient().DownloadString(searchString);
            JToken giffyToken = JObject.Parse(json);
            string url = giffyToken.SelectToken("data").SelectToken("url").ToString(); 
            string originalGif = giffyToken.SelectToken("data").SelectToken("images").SelectToken("original").SelectToken("url").ToString();
            try
            {
                string gif = GifUtilities.DoFaceReplace(originalGif);
                await Context.Channel.SendFileAsync(gif);
                return;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            await ReplyAsync(url);
        }
    }
}
