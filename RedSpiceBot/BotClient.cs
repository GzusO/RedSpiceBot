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
using System.Text.RegularExpressions;
using System.Collections.Generic;

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
        ConfigInfo configInfo;

        #region Chat Strings
        private const string SpiceBotReply = "Buy !RedSpice with channel points! " +
                        "Check your red spice stores with !MySpice.";
        private const string RedSpiceReply = "Earn Red Spice today! " +
                        "Trade it for rare artifacts sometime in the future!";
        private const string MySpiceReply = "{0}, you have {1} Red Spice.";
        #endregion

        public Bot()
        {
            // Get connection info from config.json and initialize channels
            configInfo = LoadInfo();
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
                case "spicebot":
                    Console.WriteLine("!SpiceBot command received.");
                    botClient.SendMessage(e.Command.ChatMessage.Channel, SpiceBotReply);
                    break;

                case "redspice":
                    Console.WriteLine("!RedSpice command received.");
                    botClient.SendMessage(e.Command.ChatMessage.Channel, RedSpiceReply);
                    break;

                case "myspice":
                    Console.WriteLine("!MySpice command received.");
                    Dictionary<string, UserStorage> storage = LoadStorage();
                    if (storage.TryGetValue(e.Command.ChatMessage.Username, out UserStorage userStorage))
                    {   
                        botClient.SendMessage(e.Command.ChatMessage.Channel, string.Format(MySpiceReply, e.Command.ChatMessage.DisplayName, userStorage.spice));
                    }
                    else
                    {
                        // If a user doesn't have an entry in the storage database then they have never acquired spice, so they have 0
                        botClient.SendMessage(e.Command.ChatMessage.Channel, string.Format(MySpiceReply, e.Command.ChatMessage.DisplayName, 0));
                    }
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
            // Check if the reward is a buy spice reward in the format ... (xN) where N is amount of spice bought
            if (e.Status == "ACTION_TAKEN")
            {
                Regex rxCheck = new Regex(@"\d+",
                    RegexOptions.Compiled | RegexOptions.IgnoreCase);
                MatchCollection matches = rxCheck.Matches(e.RewardTitle);
                foreach (Match match in matches)
                {
                    int amount = Int32.Parse(match.Value);
                    UpdateSpiceStorage(e.Login, amount);
                }
            }

        }
        #endregion

        #region Helpers
        private ConfigInfo LoadInfo()
        {
            using (StreamReader r = new StreamReader("../../Config/config.json"))
            {
                string config = r.ReadToEnd();
                r.Close();
                return JsonConvert.DeserializeObject<ConfigInfo>(config);
            }
        }

        private Dictionary<string, UserStorage> LoadStorage()
        {
            using (StreamReader r = new StreamReader("../../SpiceStorage/storage.json"))
            {
                string storageString = r.ReadToEnd();
                r.Close();
                return JsonConvert.DeserializeObject<Dictionary<string, UserStorage>>(storageString);
            }
        }

        /*
         * Updates a users spice amount, returning whether or not the change is legal
         * Only updates the spice count on a legal request
         * Probably has race conditions and other weird bugs, but those are future me's problems
         */
        private bool UpdateSpiceStorage(string login, int spiceChange)
        {
            bool isLegal = false;
            Dictionary<string, UserStorage> storage = LoadStorage();

            // If the storage doesn't exist at all yet, initialize it
            if (storage == null)
            {
                storage = new Dictionary<string, UserStorage>();
            }

            // Update the user's spice count
            UserStorage curStorage;
            if (storage.TryGetValue(login, out curStorage))
            {
                // The user already has a storage
                if ((curStorage.spice + spiceChange) >= 0)
                {
                    storage[login].spice = curStorage.spice + spiceChange;
                    isLegal = true;
                }
            }
            else
            {
                // The user does not have a storage set up
                if (spiceChange > 0)
                {
                    UserStorage newStorage = new UserStorage();
                    newStorage.spice = spiceChange;
                    storage.Add(login, newStorage);
                    isLegal = true;
                }
            }

            // Save the changes back to the spice storage
            // Probably dangerous to do this cause race conditions, should look into a way to prevent that
            File.WriteAllText(@"../../SpiceStorage/storage.json", JsonConvert.SerializeObject(storage));

            return isLegal;
        }
        #endregion
    }

    #region Config Classes
    public class ConfigInfo
    {
        public ConfigIdentity identity;
        public string[] channels;
        public string accessToken;
        public string clientID;
        public string refreshToken;
    }

    public class ConfigIdentity
    {
        public string username;
        public string password;
    }
    #endregion

    #region Storage Classes
    // Storage obect exists as a login-keyed dictionary of UserStorage objects
    public class UserStorage
    {
        public int spice;
        public string[] artifacts;
    }
    #endregion
}