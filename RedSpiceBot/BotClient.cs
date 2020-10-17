using Newtonsoft.Json;
using System;
using System.IO;
using TwitchLib.Client;
using TwitchLib.Client.Enums;
using TwitchLib.Client.Events;
using TwitchLib.Client.Extensions;
using TwitchLib.Client.Models;
using TwitchLib.Communication.Clients;
using TwitchLib.Communication.Models;
using TwitchLib.PubSub;
using TwitchLib.PubSub.Events;

namespace RedSpiceBot
{
    class BotClient
    {
        static void Main(string[] args)
        {
            Bot bot = new Bot();
            Console.ReadLine();
        }
    }

    /*
    * Primary class where bot stuff happens
    */
    class Bot
    {
        TwitchClient botClient;
        TwitchPubSub botPubSub;
        ConfigLoader.Info configInfo;

        public Bot()
        {
            // Get connection info from config.json and initialize channels
            configInfo = ConfigLoader.LoadInfo();
            ConnectionCredentials credentials = new ConnectionCredentials(configInfo.identity.username, configInfo.identity.password);
            var clientOptions = new ClientOptions
            {
                MessagesAllowedInPeriod = 750,
                ThrottlingPeriod = TimeSpan.FromSeconds(30)
            };
            WebSocketClient customClient = new WebSocketClient(clientOptions);
            botClient = new TwitchClient(customClient);
            foreach (string channel in configInfo.channels)
            {
                botClient.Initialize(credentials, channel);
            }

            // Client setup
            botClient.OnLog += OnLog;
            botClient.OnJoinedChannel += OnJoinedChannel;
            botClient.OnMessageReceived += OnMessageReceived;
            botClient.OnWhisperReceived += OnWhisperReceived;
            botClient.OnNewSubscriber += OnNewSubscriber;
            botClient.OnConnected += OnConnected;
            botClient.OnChatCommandReceived += OnChatCommandReceived;

            botClient.Connect();

            // PubSub setup
            botPubSub = new TwitchPubSub();

            botPubSub.OnPubSubServiceConnected += OnPubSubServiceConnected;
            botPubSub.OnListenResponse += OnListenResponse;
            botPubSub.OnStreamUp += OnStreamUp;
            botPubSub.OnStreamDown += OnStreamDown;
            botPubSub.OnRewardRedeemed += OnRewardRedeemed;

            // Diadonic's channel name/ID is just hard coded cause it's public anyways
            botPubSub.ListenToVideoPlayback("Diadonic");
            botPubSub.ListenToRewards("24384880");

            botPubSub.Connect();
        }

        #region Bot Event Handlers
        private void OnLog(object sender, TwitchLib.Client.Events.OnLogArgs e)
        {
            Console.WriteLine($"{e.DateTime}: {e.BotUsername} - {e.Data}");
        }

        private void OnConnected(object sender, OnConnectedArgs e)
        {
            Console.WriteLine($"Connected to {e.AutoJoinChannel}");
        }

        private void OnJoinedChannel(object sender, OnJoinedChannelArgs e)
        {
            Console.WriteLine("Hey guys! I am a bot connected via TwitchLib!");
            //client.SendMessage(e.Channel, "Hey guys! I am a bot connected via TwitchLib!",true);
        }

        private void OnChatCommandReceived(object sender, OnChatCommandReceivedArgs e)
        {
            switch (e.Command.CommandText.ToLower())
            {
                case "redspice":
                    Console.WriteLine("!Redspice command received.");
                    botClient.SendMessage(e.Command.ChatMessage.Channel, "Earn Redspice today!");
                    break;
                default:
                    Console.WriteLine($"Received unknown command: {e.Command.CommandText}, from {e.Command.ChatMessage.Username}.");
                    break;
            }
        }

        private void OnMessageReceived(object sender, OnMessageReceivedArgs e)
        {
            Console.WriteLine($"Received message: {e.ChatMessage.Message}, from {e.ChatMessage.Username}.");
        }

        private void OnWhisperReceived(object sender, OnWhisperReceivedArgs e)
        {
            if (e.WhisperMessage.Username == "my_friend")
                botClient.SendWhisper(e.WhisperMessage.Username, "Hey! Whispers are so cool!!");
        }

        private void OnNewSubscriber(object sender, OnNewSubscriberArgs e)
        {
            if (e.Subscriber.SubscriptionPlan == SubscriptionPlan.Prime)
                botClient.SendMessage(e.Channel, $"Welcome {e.Subscriber.DisplayName} to the channel! You just earned 500 points! So kind of you to use your Twitch Prime On this channel!");
            else
                botClient.SendMessage(e.Channel, $"Welcome {e.Subscriber.DisplayName} to the channel! You just earned 500 points!");
        }
        #endregion

        #region PubSub Handlers
        private void OnPubSubServiceConnected(object sender, EventArgs e)
        {
            // SendTopics accepts an oauth optionally, which is necessary for some topics
            botPubSub.SendTopics(configInfo.accessToken);
            Console.WriteLine($"Sent topic access token {configInfo.accessToken}.");
        }

        private void OnListenResponse(object sender, OnListenResponseArgs e)
        {
            if (!e.Successful)
                throw new Exception($"Failed to listen! Response: {e.Response}");
        }

        private void OnStreamUp(object sender, OnStreamUpArgs e)
        {
            Console.WriteLine($"Stream just went up! Play delay: {e.PlayDelay}, server time: {e.ServerTime}");
        }

        private void OnStreamDown(object sender, OnStreamDownArgs e)
        {
            Console.WriteLine($"Stream just went down! Server time: {e.ServerTime}");
        }

        private void OnRewardRedeemed(object sender, OnRewardRedeemedArgs e)
        {
            Console.WriteLine("Points redeemed");
            Console.WriteLine(e.DisplayName);
        }
        #endregion
    }

    /*
    * Static class that exists solely to load username and authentication token from the config JSON file
    */
    class ConfigLoader
    {
        static public Info LoadInfo()
        {
            using (StreamReader r = new StreamReader("../../Config/config.json"))
            {
                string config = r.ReadToEnd();
                return JsonConvert.DeserializeObject<Info>(config);
            }
        }

        public class Info
        {
            public Identity identity;
            public string[] channels;
            public string accessToken;
            public string clientID;
            public string refreshToken;
        }

        public class Identity
        {
            public string username;
            public string password;
        }
    }
    
}