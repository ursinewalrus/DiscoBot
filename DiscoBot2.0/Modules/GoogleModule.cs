using System;
using System.Collections.Generic;
using System.Text;
using Discord;
using Discord.Commands;
using System.Threading.Tasks;
using System.Linq;
using System.Net;
using Discobot.Utilities;
using HtmlAgilityPack;

namespace Discobot.Modules
{
    [Name("Google")]
    public class GoogleModule : ModuleBase
    {

        [Command("googleim")]
        //dubious command ethicacy , also remove second arg, use remainder for search?
        public async Task GoogleImageSearch([Remainder]string search)
        {
            string searchFor = Uri.EscapeDataString(search);
            var html = new WebClient().DownloadString("https://www.google.com/search?q=" + searchFor + "&tbm=isch&start=1");
            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(html);
            var imgNodes = document.DocumentNode.Descendants("img");
            List<string> images = new List<string>();
            List<string> parentLinks = new List<string>();
            foreach(HtmlNode img in imgNodes)
            {
                var parent = img.ParentNode.Attributes["href"].Value;
                var src = img.Attributes["src"].Value;
                images.Add(src);
                parentLinks.Add(parent);
            }
            ;
            await ReplyAsync(images[0]);

        }
    }
}
