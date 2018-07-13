using System;
using System.Collections.Generic;
using System.Text;
using Discord;
using Discord.Commands;
using System.Threading.Tasks;
using System.Linq;
using Discobot.Utilities;
using System.Text.RegularExpressions;

namespace Discobot.Modules
{
    [Name("SpeakMod")]
    public class SpeakModModule : ModuleBase
    {

        [RequireBotPermission(GuildPermission.ManageMessages)]
        //[RequireUserPermission(GuildPermission.ManageMessages)]
        [Command("memspeak")]
        public async Task MemSpeak([Remainder]string input)
        {
            await ModuleUtilities.DeleteMessage(Context);
            string oldMsg = input;
            string newMsg = "";
            newMsg += Context.User.Username.ToString() + ": ";

            for (int i = 0; i < oldMsg.Length; i++)
            {
                newMsg += oldMsg[i] + (i < oldMsg.Length - 1 ? "(" : "");
            }
            newMsg += new string(')', oldMsg.Length - 1);
            await ReplyAsync(newMsg);
        }

        [RequireBotPermission(GuildPermission.ManageMessages)]
        [Command("mockbob")]
        public async Task MockBob([Remainder]string input)
        {
            await ModuleUtilities.DeleteMessage(Context);
            string oldMsg = input;

            string newMsg = "";
            newMsg += Context.User.Username.ToString() + ": ";

            for(int i = 0; i < oldMsg.Length; i++)
            {
                newMsg += (i % 2 == 0 ? Char.ToUpper(oldMsg[i]) : Char.ToLower(oldMsg[i]));
            }
            await ReplyAsync(newMsg +"\n\r" +"http://i.imgflip.com/1rn9v3.jpg");
        }

        [RequireBotPermission(GuildPermission.ManageMessages)]
        [Command("scrustspeak")]
        public async Task Scrust(int maxChars, [Remainder]string input)
        {
            await ModuleUtilities.DeleteMessage(Context);
            string oldMsg = input;

            string newMsg = "";
            newMsg += Context.User.Username.ToString() + ": ";
        
            string[] diacriticalPrefixes = { @"\u030", @"\u031", @"\u032", @"\u033", @"\u034", @"\u035", @"\u036" };

            Random random = new Random();
            var scrustyString = "";
            foreach(char c in input)
            {
                if (c == ' ')
                {
                    scrustyString += " ";
                    continue;
                }
                string modChar = c.ToString();
                int toAdd = random.Next(0, maxChars);
                var diaCrits = "";
                for (int i = 0; i < toAdd; i++)
                {
                    var firstPart = diacriticalPrefixes[random.Next(0, 7)];
                    var secondPart = random.Next(0, 16).ToString("X");
                    diaCrits = Regex.Unescape((diaCrits + (firstPart  + secondPart) )).Normalize();
                    ;
                }
                modChar = Regex.Unescape((modChar + diaCrits)).Normalize();
                scrustyString += modChar;
            }
            await ReplyAsync(scrustyString);
        }

        [RequireBotPermission(GuildPermission.ManageMessages)]
        [Command("muddlespeak")]
        public async Task Muddle([Remainder] string input)
        {
            await ModuleUtilities.DeleteMessage(Context);

            List<string> words = input.Split(' ').ToList();

            string muddledSentance = "";
            //golf it because
            //having it in the select like that, must do some sort of caching, same result for same input in string every time, desirable?
            muddledSentance += String.Join(String.Empty, words.Select(w => { return Scramble(w) + " "; } ));
            await ReplyAsync(muddledSentance);

        }


        //   for i from n−1 downto 1 do
        //j ← random integer such that 0 ≤ j ≤ i
        //exchange a[j] and a[i]
        //12435 
        public static string Scramble(string word)
        {
            if(word.Length < 4)
            {
                return word;
            }
            else
            {
                Random rand = new Random();
                List<char> letters = word.ToCharArray().ToList();
                for (int i = letters.Count() - 2; i>=1; i--)
                {
                    int j = rand.Next(1, i);
                    char temp = letters[i];
                    letters[i] = letters[j];
                    letters[j] = temp;
                }
                return String.Join(String.Empty, letters);
            };
        }

        //public async Task DeleteMessage(IMessage message)
        //{
        //    IMessage[] msg = { message };
        //    await Context.Channel.DeleteMessagesAsync(msg);
        //}

        //public string TrimCommand(IMessage message)
        //{
        //    List<string> oldMsgArr = Context.Message.ToString().Split(' ').ToList();
        //    return String.Join(" ", oldMsgArr.GetRange(1, oldMsgArr.Count - 1));
        //}


    }
}
