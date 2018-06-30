using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;
using Newtonsoft.Json;
using Discobot.Objects;
using Discobot.Utilities;

//doesnt really interject yet...gotta figure that one out
namespace Discobot.Modules
{
    [Name("Interject")]
    public class InterjectModule : ModuleBase
    {
        [Command("haiku")]
        public async Task AlternateHaiku([Remainder]string input)
        {
            List<SimilarMeanings> haikuWords = InterjectUtilities.FormatHaikuInput(input);
            //maybe for listen, refactor this into FormatHaikuInput
            string haiku = "";
            //here lets choose linebreaks
            foreach (SimilarMeanings word in haikuWords)
            {
                haiku += word.word + " " + word.newline;
            }
            await ReplyAsync(haiku);

        }
    }
}
