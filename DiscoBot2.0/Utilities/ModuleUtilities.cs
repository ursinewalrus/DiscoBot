using System;
using System.Collections.Generic;
using System.Text;
using Discord;
using Discord.Commands;
using System.Threading.Tasks;
using System.Linq;
using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Discobot.Utilities
{
    static class ModuleUtilities
    {

        public enum ImageLocations {
            Message,
            Preview,
            Attachment,
            None
        }

        public static async Task DeleteMessage(ICommandContext Context)
        {
            IMessage[] msg = { Context.Message };
            await Context.Channel.DeleteMessagesAsync(msg);
        }

        public static string TrimCommand(ICommandContext Context)
        {
            List<string> oldMsgArr = Context.Message.ToString().Split(' ').ToList();
            return String.Join(" ", oldMsgArr.GetRange(1, oldMsgArr.Count - 1));
        }

        public static object GetJsonFromEndpoint(string endpoint)
        {
            var json = new WebClient().DownloadString(endpoint);
            JToken token = JObject.Parse(json);
            return token;
            //object jsonObj = new JavaScriptSerializer().DeserializeObject(json);
            //return jsonObj;
        }
    }
}
