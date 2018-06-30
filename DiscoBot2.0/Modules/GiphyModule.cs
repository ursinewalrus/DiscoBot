using System;
using System.Collections.Generic;
using System.Text;
using Discord;
using Discord.Commands;
using System.Threading.Tasks;
using System.Linq;
using Discobot.Utilities;
using System.Configuration;
using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using GifProcessing;

namespace Discobot.Modules
{

    [Name("Giphy")]
    public class GiphyModule : ModuleBase
    {
        private string GiphyKey => ConfigurationManager.AppSettings["giphykey"];

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
                GifProcessing.GifProcessing processor = new GifProcessing.GifProcessing("ImageManipulation");
                string dlLocation = processor.DownloadGif(originalGif);
                var newGif = processor.GifFaceSwap("me.jpg", "C:\\Users\\jkerxhalli\\source\\repos\\DiscoBot2.0\\DiscoBot2.0\\ImageManipulation\\TempGifs\\giphy.gif");
                await Context.Channel.SendFileAsync(newGif);
                return;
                ;
            }
            catch (Exception e)
            {
                ;
                Console.WriteLine(e);
            }
                ;
            await ReplyAsync(url);
        }
    }
}
