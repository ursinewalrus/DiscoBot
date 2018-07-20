using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
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
            List<WordInfo> haikuWords = InterjectUtilities.FormatHaikuInput(input);
            //maybe for listen, refactor this into FormatHaikuInput
            string haiku = "";
            //here lets choose linebreaks
            foreach (WordInfo word in haikuWords)
            {
                haiku += word.word + " " + word.newline;
            }
            await ReplyAsync(haiku);

        }


        //maybe allow some customization as to the types of limrick made
        [Command("limrick")]
        public async Task Limrick([Remainder]string input)
        {
            List<string> words = input.Split(' ').ToList();
            var limrick = InterjectUtilities.BuildLimrick(words);
            await ReplyAsync(limrick);

        }
    }
}
