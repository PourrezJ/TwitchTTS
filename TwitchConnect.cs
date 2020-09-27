using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Speech.Synthesis;
using System.Windows.Forms;
using TwitchLib.Api;
using TwitchLib.Api.Core.Enums;
using TwitchLib.Api.Events;
using TwitchLib.Client;
using TwitchLib.Client.Events;
using TwitchLib.Client.Models;
using TwitchLib.Communication.Clients;
using TwitchLib.Communication.Models;

namespace TwitchTTS
{
    internal class TwitchConnect : IDisposable
    {
        internal static TwitchAPI API;
        internal static TwitchClient Client;
        internal static string ChannelName;
        internal static SpeechSynthesizer Synthesizer;

        internal static Dictionary<string, DateTime> Viewers;

        public TwitchConnect(string name)
        {
            if (Program.Config.LastChannel != name)
            {
                Program.Config.LastChannel = name;
                Program.Config.Save();
            }

            Viewers = new Dictionary<string, DateTime>();

            Synthesizer = new SpeechSynthesizer();
            Synthesizer.SetOutputToDefaultAudioDevice();
            Synthesizer.Rate = Program.Config.VoiceRate;
            Synthesizer.Volume = Program.Config.Volume;


            if (!string.IsNullOrEmpty(Program.Config.Culture))
            {
                var voice = Synthesizer.GetInstalledVoices().FirstOrDefault(p => p.VoiceInfo.Culture.ToString().ToLower() == Program.Config.Culture.ToLower());

                if (voice != null)
                    Synthesizer.SelectVoice(voice.VoiceInfo.Name);
            }

            ChannelName = name;

            API = new TwitchLib.Api.TwitchAPI();
            API.ThirdParty.AuthorizationFlow.OnUserAuthorizationDetected += AuthorizationFlow_OnUserAuthorizationDetected;
            API.ThirdParty.AuthorizationFlow.OnError += AuthorizationFlow_OnError;

            var flow = API.ThirdParty.AuthorizationFlow.CreateFlow(
               "Twitch TTS",
               new List<AuthScopes>()
               {
                    AuthScopes.Chat_Login,
                    //AuthScopes.Channel_Read,
                    //AuthScopes.User_Read
               }
            );
            Main.Instance.ButtonChangeText("Connexion ...");

            if (!string.IsNullOrEmpty(Program.Config.Token))
            {
                Connect(Program.Config.Token);
            }
            else
            {
                System.Diagnostics.Process.Start(flow.Url);

                API.ThirdParty.AuthorizationFlow.BeginPingingStatus(flow.Id);

                API.ThirdParty.AuthorizationFlow.GetAccessToken();
            }
        }

        private static void Client_OnLog(object sender, OnLogArgs e)
        {
            Debug.WriteLine($"{e.DateTime.ToString()}: {e.BotUsername} - {e.Data}");
        }

        private static void Client_OnConnected(object sender, OnConnectedArgs e)
        {
            Main.Instance.ButtonChangeText("Connected");
            Debug.WriteLine($"Connected to {e.AutoJoinChannel}");
        }

        private static void AuthorizationFlow_OnUserAuthorizationDetected(object sender, TwitchLib.Api.Events.OnUserAuthorizationDetectedArgs e)
        {
            Debug.WriteLine($"User authorization detected! Details");
            Debug.WriteLine($" - Authorization flow: {e.Id}");
            Debug.WriteLine($" - Scopes: {String.Join(", ", e.Scopes)}");
            Debug.WriteLine($" - Twitch Username: {e.Username}");
            Debug.WriteLine($" - Access Token: {e.Token}");
            Debug.WriteLine($" - Refresh Token: {e.Refresh}");

            Connect(e.Token);
            Program.Config.Token = e.Token;
            Program.Config.Save();
        }

        private static void Client_OnChatCommandReceived(object sender, OnChatCommandReceivedArgs e)
        {
            if (e.Command.CommandText == Program.Config.Command)
            {
                string viewerName = e.Command.ChatMessage.DisplayName;

                if (Program.Config.SubOnly && !e.Command.ChatMessage.IsSubscriber)
                {
                    if (!e.Command.ChatMessage.IsModerator)
                        return;
                }

                if (Program.Config.ModoOnly && !e.Command.ChatMessage.IsModerator)
                    return;

                if (!e.Command.ChatMessage.IsModerator)
                {
                    foreach (var badworld in Program.Config.BadWord)
                    {
                        if (e.Command.ArgumentsAsString.Contains(badworld))
                        {
                            Client.SendMessage(e.Command.ChatMessage.Channel, $"@{viewerName} You use illegal badword: {e.Command.ArgumentsAsString}.");
                            return;
                        }
                    }
                }

                if (!Viewers.ContainsKey(viewerName))
                    Viewers.Add(viewerName, DateTime.Now);

                if (Viewers[viewerName] <= DateTime.Now)
                {
                    Viewers[viewerName] = DateTime.Now.AddSeconds(Program.Config.Delay);

                    Debug.WriteLine(e.Command.ArgumentsAsString);
                    Synthesizer.Speak($"{e.Command.ChatMessage.DisplayName} {Program.Config.SpeakPrefix} {e.Command.ArgumentsAsString}");
                }
                else
                    Client.SendMessage(e.Command.ChatMessage.Channel, $"@{viewerName} you can't send your TTS.");

            }
            else if (e.Command.CommandText == "ttsaddbadword" && e.Command.ChatMessage.IsModerator)
            {
                Program.Config.BadWord.Add(e.Command.ArgumentsAsString);
                Program.Config.Save();
                Client.SendMessage(e.Command.ChatMessage.Channel, $"{e.Command.ArgumentsAsString} added in the bad word list.");
            }
        }

        private void AuthorizationFlow_OnError(object sender, OnAuthorizationFlowErrorArgs e)
        {
            MessageBox.Show("Authentification Error");
        }

        public static void Connect(string token)
        {
            ConnectionCredentials credentials = new ConnectionCredentials(ChannelName, token);
            var clientOptions = new ClientOptions
            {
                MessagesAllowedInPeriod = 750,
                ThrottlingPeriod = TimeSpan.FromSeconds(30)
            };
            WebSocketClient customClient = new WebSocketClient(clientOptions);
            Client = new TwitchClient(customClient);
            Client.Initialize(credentials, ChannelName);

            Client.OnLog += Client_OnLog;
            Client.OnConnected += Client_OnConnected;
            Client.OnChatCommandReceived += Client_OnChatCommandReceived;
            Client.AddChatCommandIdentifier('!');

            Client.Connect();
        }

        public void Dispose()
        {
            Client.Disconnect();
            Synthesizer.Dispose();
        }
    }
}
