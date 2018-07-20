using Discord.Commands;
using System.Threading.Tasks;
using Discobot.Utilities;
using System.Collections.Generic;

//https://github.com/overtools/OWLeagueLib
//https://api.overwatchleague.com
//https://us.battle.net/forums/en/bnet/topic/20761746764
///rankings, /schedule, and /matches /teams /news /teams/:teamId /nes/:blogID
///https://api.overwatchleague.com
namespace Discobot.Modules
{
    [Name("OWL")]
    public class OWLModule : ModuleBase
    {
        private string OwlUrl = "https://api.overwatchleague.com/";

        [Command("owl-rankings")]
        public async Task Rankings(string input = "noIm")
        {
            if(input != "noIm" && input != "im")
            {
                input = "noIm";
            }
            dynamic result = ModuleUtilities.GetJsonFromEndpoint(OwlUrl + "rankings");
            //var content = result["content"];
            List<string> teams = OWLUtilities.ParseTeamInfo(result,input);
            if (input == "im")
            {
                foreach (string team in teams)
                {
                    await ReplyAsync(team);
                }
            }
            else if(input == "noIm")
            {
                string outputString = "";
                foreach (string team in teams)
                {
                    outputString += team;
                }
                await ReplyAsync(outputString);

            }

        }
        
        [Command("owl-player")]
        public async Task Player(string player = "default")
        {
            dynamic result = ModuleUtilities.GetJsonFromEndpoint(OwlUrl + "players");
            string playerData = OWLUtilities.GetPlayerData(result,player);
            await ReplyAsync(playerData)
            ;
        }
    }
}
