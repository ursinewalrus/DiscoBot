//using System;
//using System.Collections.Generic;
//using System.Text;
//using Discord;
//using Discord.Commands;
//using System.Threading.Tasks;
//using System.Linq;
//using discobot.Utilities;
//using System.Configuration;
//using System.Web.Script.Serialization;
//using System.Net;

//namespace discobot.Modules
//{

//    [Name("NASA")]
//    public class NASAModule : ModuleBase
//    {
//        private string SearchString => "https://api.nasa.gov/planetary/apod?api_key=" +  ConfigurationManager.AppSettings["nasakey"];

//        //[Command("nasa")]
//        //public async Task NASA(string input = "nope")
//        //{
//        //    bool showExplanation = (input == "ex" ? true : false);
//        //    var json = new WebClient().DownloadString(SearchString);
//        //    dynamic jsonObj = new JavaScriptSerializer().DeserializeObject(json);
//        //    string output = (showExplanation ? jsonObj["title"] + "\n\r" + jsonObj["explanation"] + "\n\r" : "\n\r") + jsonObj["url"];
//        //    await ReplyAsync(output);
//        //}
//    }
//}
