using System;
using System.Collections.Generic;
using System.Text;
using Discord;
using Discord.Commands;
using System.Threading.Tasks;
using Discobot.Utilities;
using System.Diagnostics;
using Discord.Audio;
using System.Reflection;

//https://github.com/mrousavy/DiscordMusicBot/tree/master/DiscordMusicBot
namespace Discobot.Modules
{
    [Name("Voice")]
    public class VoiceModule : ModuleBase
    {
        private IAudioClient _audioClient;
        private bool _audioLocked = false;
        private string path => System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location + "\\soundbite.wav");

        [Command("join", RunMode = RunMode.Async)]
        public async Task JoinChannel(IVoiceChannel channel = null)
        {
            // Get the audio channel
            channel = channel ?? (Context.Message.Author as IGuildUser)?.VoiceChannel;
            if (channel == null)
            {
                await Context.Message.Channel.SendMessageAsync("User must be in a voice channel, or a voice channel must be passed as an argument.");
                return;
            }
            // For the next step with transmitting audio, you would want to pass this Audio Client in to a service.
            _audioClient = await channel.ConnectAsync();
        }

        public void GenerateWaveFileFromText(string text, int rate = 0)
        {
            //var speechSynth = new System.Speech.Synthesis.SpeechSynthesizer();
            //speechSynth.SelectVoice("Microsoft Zira Desktop");
            //speechSynth.Rate = rate;
            //speechSynth.SetOutputToWaveFile(System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location + "\\soundbite.wav"));
            //speechSynth.Speak(text);
            //speechSynth.Dispose();
        }

        [Command("speak", RunMode = RunMode.Async)]
        public async Task SpeakInChannel(string input, int rate = 0)
        {
            if (!_audioLocked)
            {
                _audioLocked = true;
                GenerateWaveFileFromText(input, rate);
                var ffmpegStream = CreateStream();
                var output = ffmpegStream.StandardOutput.BaseStream;
                var discord = _audioClient.CreatePCMStream(AudioApplication.Mixed);
                await output.CopyToAsync(discord);
                await discord.FlushAsync();
            }
        }
        private Process CreateStream()
        {
            var ffmpeg = new ProcessStartInfo
            {
                FileName = "ffmpeg",
                Arguments = $"-i {path} -ac 2 -f s16le -ar 48000 pipe:1",
                UseShellExecute = false,
                RedirectStandardOutput = true,
            };
            return Process.Start(ffmpeg);
        }
       
    }
}
