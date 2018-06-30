using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using System.ComponentModel;
using System.Configuration;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using System.Linq;
using Discobot.Utilities;
using System.Collections.Generic;
using Discobot.Modules;
using Mono.Reflection;
//https://discord.foxbot.me/docs/guides/commands/commands.html
//https://github.com/Aux/Discord.Net-Example
//https://media.readthedocs.org/pdf/discordnet-foxbot-docs/legacy/discordnet-foxbot-docs.pdf
//https://github.com/RogueException/Discord.Net
namespace Discobot
{
    class Program
    {
        static BackgroundWorker Worker = new BackgroundWorker();
        string token = ConfigurationManager.AppSettings["apiKey"];

        private CommandService commands = new CommandService();
        private DiscordSocketClient client = new DiscordSocketClient();
        private IServiceProvider services = new ServiceCollection().BuildServiceProvider();
        //private ServiceCollection servicesProvider = new ServiceCollection();

        static void Main(string[] args) => new Program().Start().GetAwaiter().GetResult();

        public async Task Start()
        {
            await client.LoginAsync(TokenType.Bot, token);
            await client.StartAsync();
            await InstallCommands();
            var c = commands;
            await Task.Delay(-1);
        }

        public async Task InstallCommands()
        {
            client.MessageReceived += HandleCommands;
            await commands.AddModulesAsync(Assembly.GetEntryAssembly());

        }

        public async Task HandleCommands(SocketMessage messageParam)
        {
            var message = messageParam as SocketUserMessage;

            var userName = message.Author.Username;

            if (message == null) return;

            int argPos = 0;

            #region command_or_at
            if (!(message.HasCharPrefix('!', ref argPos) || message.HasMentionPrefix(client.CurrentUser, ref argPos)) && !overRideDefaultAllowances(message))
            {
                return;
            }
            #endregion

            string channelFrom = message.Channel.Name;
            //move to config
            List<string> allowedChannels = new List<string>(new string[] { "bot-channel" });

            if (message.Content.Contains(".gif"))
            {
                //GifProcessing.GifProcessing gifProcessor = new GifProcessing.GifProcessing();
                //string dlLocation = gifProcessor.downloadGif(message.Content);
            }

            #region wrong_channel
            if (!allowedChannels.Exists(s => string.Equals(s, channelFrom, StringComparison.OrdinalIgnoreCase)) && !overRideDefaultAllowances(message))
            {
                var wrongContext = new CommandContext(client, message);
                await wrongContext.Channel.SendMessageAsync(("WRONG CHANNEL"));
                return;
            }
            #endregion

            #region intercept_scrustspeak
            if (overRideDefaultAllowances(message) && !message.HasCharPrefix('!', ref argPos))
            {
                PropertyInfo propInfo = typeof(SocketUserMessage).GetProperty("Content");
                FieldInfo contentField = propInfo.GetBackingField();
                argPos++;
                contentField.SetValue(message, "!scrustspeak 4 " + message.Content);
            }
            #endregion

            var context = new CommandContext(client, message);

            var typing = context.Channel.EnterTypingState();
            var result = await commands.ExecuteAsync(context, argPos, services);
            typing.Dispose();

            if (!result.IsSuccess)
            {
                await context.Channel.SendMessageAsync(result.ErrorReason);
            }

        }

        public bool overRideDefaultAllowances(SocketUserMessage message)
        {
            List<string> interuptUsers = ConfigurationManager.AppSettings["overrideUsers"].Split(new string[] {", " }, StringSplitOptions.None).ToList();
            var userName = message.Author.Username;

            if (interuptUsers.Exists(u => string.Equals(u, userName, StringComparison.OrdinalIgnoreCase)))
            {
                return true;
            }
            return false;
        }
    }
}
