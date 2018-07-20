using Discord.Commands;
using System.Threading.Tasks;
using TweetSharp;

namespace Discobot.Modules
{
    [Name("Tweety")]
    public class TweetModule : ModuleBase
    {

        [Command("tweet")]
        public async Task Tweet()
        {
            var bot_id = Context.User;
            ;
            await ReplyAsync("A");
        }
    }
}
