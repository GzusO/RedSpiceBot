using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Timers;
using TwitchLib.Api;
using TwitchLib.Client;
using TwitchLib.Client.Enums;
using TwitchLib.Client.Events;
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
        private TwitchClient botClient;
        private TwitchPubSub botPubSub;
        private static TwitchAPI botAPI;
        private ConfigInfo configInfo;

        private Dictionary<string, string> chatCommands;
        private const string commandDataPath = @"./SpiceStorage/commands.json";
        private const string configDataPath = @"./Config/config.json";
        private const string userDataPath = @"../../SpiceStorage/storage.json";

        private Dictionary<string, UserStorage> userStorage;
        private List<Artifact> curArtifacts;
        private Dictionary<int, Artifact> prevArtifacts;
        private bool ArtifactDisplayCooldown = false;
        private  System.Timers.Timer ArtifactCooldownTimer = new System.Timers.Timer(30 * 1000); // Change this to alter the cooldown timer

        #region Chat Strings
        private const string SpiceBotReply = "Buy !RedSpice with channel points! " +
                        "Check your red spice stores with !MySpice. " +
                        "Check other people's spice with !YourSpice <name>. " +
                        "You can also buy !artifacts, using !artifacts help for more commands.";
        private const string RedSpiceReply = "Earn Red Spice today! " +
                        "Trade it for rare artifacts sometime in the future!";

        private const string MySpiceReply = "{0}, you have {1} Red Spice.";
        private const string NoStorageReply = "{0} doesn't have a Red Spice account set up yet. " +
                        "Buy any amount of Red Spice to start your account!";

        private const string ArtifactHelp = "Check current artifacts with !artifacts, buy an artifact " +
                        "with !artifacts buy <ID>, and check a user's artifacts with !artifacts check <username>.";
        private const string ArtifactInvalidPurchase = "There is no artifact with an ID of {0} for sale.";
        private const string ArtifactInvalid = "There is no artifact with an ID of {0}.";
        private const string ArtifactNotEnoughSpice = "The artifact \"{0}\" costs {1} spice, " +
                        "{2} only has {3} spice.";
        private const string ArtifactNoneInAccount = "{0} doesn't own any artifacts!";
        #endregion

        public Bot()
        {
            // Get connection info from config.json and initialize channels
            configInfo = ConfigInfo.LoadInfo(configDataPath);
            chatCommands = ChatCommand.LoadFromJson(commandDataPath);
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

            // API setup
            botAPI = new TwitchAPI();
            botAPI.Settings.ClientId = configInfo.clientID;
            botAPI.Settings.AccessToken = configInfo.accessToken;

            // Load user storage and artifact history state
            userStorage = LoadStorage();
            curArtifacts = Artifact.GenerateArticats(out prevArtifacts);
            if (userStorage == null)
			{
                userStorage = new Dictionary<string, UserStorage>();

            }

            // Set up timer callback
            ArtifactCooldownTimer.Elapsed += ResetArtifactCooldown;
            ArtifactCooldownTimer.AutoReset = false;
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

        private async void OnChatCommandReceived(object sender, OnChatCommandReceivedArgs e)
        {
            TwitchLib.Api.V5.Models.Users.Users user;
            string message = "";
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
                case "yourspice":
                    Console.WriteLine("!MySpice/!YourSpice command received.");
                    if (e.Command.CommandText.ToLower() == "yourspice")
                    {
                        if (e.Command.ArgumentsAsList.Count != 1) { return; }
                        user = await UsernameToUser(e.Command.ArgumentsAsList[0]);
                        if (user.Matches.Length == 1) { DisplaySpice(user.Matches[0].Id, user.Matches[0].DisplayName, e.Command.ChatMessage.Channel); }
                    }
                    else
                    {
                        DisplaySpice(e.Command.ChatMessage.UserId, e.Command.ChatMessage.DisplayName, e.Command.ChatMessage.Channel);
                    }
                    break;

                case "artifacts":
                    Console.WriteLine("!artifacts command received.");
                    HandleArtifactChatCommands(e);
                    break;

                case "modspice": // !modspice <name> <amount>
                    if (!e.Command.ChatMessage.IsModerator || e.Command.ArgumentsAsList.Count != 2) { return; } // Mod-only command to manually fix people's spice
                    Console.WriteLine("!ModSpice command received.");
                    user = await UsernameToUser(e.Command.ArgumentsAsList[0]);
                    if (!Int32.TryParse(e.Command.ArgumentsAsList[1], out int spiceChange)) { return; }
                    if (user.Matches.Length == 1) { UpdateSpiceStorage(ref userStorage, user.Matches[0].Id, user.Matches[0].DisplayName, spiceChange); }
                    break;

                case "addcommand":
                    if (!(e.Command.ChatMessage.IsModerator || e.Command.ChatMessage.IsBroadcaster) || e.Command.ArgumentsAsList.Count <= 1)
                        return;
                    if (chatCommands.ContainsKey(e.Command.ArgumentsAsList[0]))
                        return;
                    chatCommands.Add(e.Command.ArgumentsAsList[0], e.Command.ArgumentsAsString.Substring(e.Command.ArgumentsAsString.IndexOf(' ')));
                    ChatCommand.SaveToJson(commandDataPath, chatCommands);
                    break;
                case "removecommand":
                    if (!(e.Command.ChatMessage.IsModerator || e.Command.ChatMessage.IsBroadcaster) || e.Command.ArgumentsAsList.Count != 1)
                        return;
                    chatCommands.Remove(e.Command.ArgumentsAsList[0]);
                    ChatCommand.SaveToJson(commandDataPath, chatCommands);
                    break;
                case "updatecommand":
                    if (!(e.Command.ChatMessage.IsModerator || e.Command.ChatMessage.IsBroadcaster) || e.Command.ArgumentsAsList.Count <= 1)
                        return;
                    if (chatCommands.ContainsKey(e.Command.ArgumentsAsList[0]))
                        chatCommands[e.Command.ArgumentsAsList[0]] = e.Command.ArgumentsAsString.Substring(e.Command.ArgumentsAsString.IndexOf(' '));
                    ChatCommand.SaveToJson(commandDataPath, chatCommands);
                    break;
                case "commands":
                    message = "Chat Commands\n";
                    foreach (string key in chatCommands.Keys)
                    {
                        message += $"!{key}\n";
                    }
                    botClient.SendMessage(e.Command.ChatMessage.Channel, message);
                    break;
                default:
                    if (chatCommands.TryGetValue(e.Command.CommandText.ToLower(), out message))
                        botClient.SendMessage(e.Command.ChatMessage.Channel, message);
                    else
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

        private async void OnRewardRedeemed(object sender, OnRewardRedeemedArgs e)
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
                    TwitchLib.Api.V5.Models.Users.Users user = await UsernameToUser(e.DisplayName);
                    UpdateSpiceStorage(ref userStorage, user.Matches[0].Id, user.Matches[0].DisplayName, amount);
                }
            }

        }
        #endregion

        #region API Calls
        private async Task<TwitchLib.Api.V5.Models.Users.Users> UsernameToUser(string name)
        {
            return await botAPI.V5.Users.GetUserByNameAsync(name);
        }
        #endregion

        /*
         * Loads a userID-keyed dictionary of storages
         */
        private Dictionary<string, UserStorage> LoadStorage()
        {
            Dictionary<string, UserStorage> storage = UserStorage.LoadStorage(userDataPath);

            // If the storage doesn't exist at all yet, initialize it
            if (storage == null)
            {
                storage = new Dictionary<string, UserStorage>();
            }

            return storage;
        }

        /*
         * Updates a users spice amount, returning whether or not the change is legal
         * Only updates the spice count on a legal request
         */
        private bool UpdateSpiceStorage(ref Dictionary<string, UserStorage> storage, string userID, string userDisplay, int spiceChange)
        {
            bool isLegal = false;

            // Update the user's spice count
            UserStorage curStorage;
            if (storage.TryGetValue(userID, out curStorage))
            {
                // The user already has a storage
                if ((curStorage.spice + spiceChange) >= 0)
                {
                    storage[userID].spice = curStorage.spice + spiceChange;
                    isLegal = true;
                }
            }
            else
            {
                // The user does not have a storage set one up
                if (spiceChange > 0)
                {
                    UserStorage newStorage = new UserStorage();
                    newStorage.spice = spiceChange;
                    newStorage.displayName = userDisplay;
                    newStorage.artifacts = new List<Artifact>();
                    storage.Add(userID, newStorage);
                    isLegal = true;
                }
            }

            // Save the changes back to the spice storage on every change

            SaveSpiceStorage(storage);
            return isLegal;
        }

        private void SaveSpiceStorage(Dictionary<string, UserStorage> storage)
        {
            File.WriteAllText(userDataPath, JsonConvert.SerializeObject(storage));
        }

        private void DisplaySpice(string userID, string userDisplay, string channel)
        {
            // Load storage and respond with how much spice the user has, if they have an account
            Dictionary<string, UserStorage> storage = UserStorage.LoadStorage(userDataPath);
            if (storage.TryGetValue(userID, out UserStorage userStorage))
            {
                botClient.SendMessage(channel, string.Format(MySpiceReply, userDisplay, userStorage.spice));
            }
            else
            {
                botClient.SendMessage(channel, string.Format(NoStorageReply, userDisplay));
            }
        }

        /*
         * Central function to parse through an handle all artifact chat commands
         * Needs cleanup/modularization but I can't be bothered to do that right now
         */
        private async void HandleArtifactChatCommands(OnChatCommandReceivedArgs e)
        {
            if (e.Command.ArgumentsAsList.Count == 0) // No arguments just sends a list of current artifacts for sale
            {
                if (ArtifactDisplayCooldown) { return; } // Ignore this command while on cooldown

                foreach (Artifact art in curArtifacts)
                {
                    botClient.SendMessage(e.Command.ChatMessage.Channel, Artifact.ToChat(art));
                }

                // Have this on an arbitrary 30 second cooldown to avoid chat spam
                ArtifactDisplayCooldown = true;
                ArtifactCooldownTimer.Start();
            }

            if (e.Command.ArgumentsAsList.Count == 1 && e.Command.ArgumentsAsList[0].ToLower() == "help") // !artifacts help
            {
                    botClient.SendMessage(e.Command.ChatMessage.Channel, ArtifactHelp);
            }

            if (e.Command.ArgumentsAsList.Count == 2 && e.Command.ArgumentsAsList[0].ToLower() == "buy") // !artifacts buy <ID>
            {
                // Check if the ID they are trying to buy is valid
                bool isValid = false;
                int index = 0;
                for (int i = 0; i < curArtifacts.Count; i++)
                {
                    if (!Int32.TryParse(e.Command.ArgumentsAsList[1], out int artID)) { break; } // if the input ID isn't even an int don't try to find it
                    if (curArtifacts[i].ID == artID)
                    { 
                        // If the ID is a valid int and matches an artifact for sale set the valid flag and stop searching
                        isValid = true;
                        index = i;
                        break;
                    } 
                }

                // Response for invalid ID
                if (!isValid) 
                { 
                    botClient.SendMessage(e.Command.ChatMessage.Channel, string.Format(ArtifactInvalidPurchase, e.Command.ArgumentsAsList[1]));
                    return;
                }

                // Check first if the user even has a spice account
                if (!userStorage.TryGetValue(e.Command.ChatMessage.UserId, out UserStorage storage)) 
                {
                    botClient.SendMessage(e.Command.ChatMessage.Channel, NoStorageReply);
                    return;
                }

                // If everything checks out, attempt to make the transaction
                if (curArtifacts[index].Value <= storage.spice) // If has enough spice to buy the artifact
                {
                    // Update the user's new spice amount and add the artifact to their list
                    UpdateSpiceStorage(ref userStorage, e.Command.ChatMessage.UserId, e.Command.ChatMessage.DisplayName, -curArtifacts[index].Value);
                    AddArtifact(ref userStorage, e.Command.ChatMessage.UserId, curArtifacts[index]);
                    Artifact.SaveToHistory(curArtifacts[index]);
                }
                else
                {
                    // Shame the user for trying to buy an artifact they can't afford
                    botClient.SendMessage(e.Command.ChatMessage.Channel, string.Format(
                        ArtifactNotEnoughSpice,
                        curArtifacts[index].Name,
                        curArtifacts[index].Value,
                        e.Command.ChatMessage.DisplayName,
                        storage.spice));
                }
            }

            if (e.Command.ArgumentsAsList.Count == 2 && e.Command.ArgumentsAsList[0].ToLower() == "check") // !artifacts check <username>
            {
                // Check if the user being checked has a spice account
                TwitchLib.Api.V5.Models.Users.Users user = await UsernameToUser(e.Command.ArgumentsAsList[1]);
                if (!userStorage.TryGetValue(user.Matches[0].Id, out UserStorage storage))
                {
                    botClient.SendMessage(e.Command.ChatMessage.Channel, string.Format(NoStorageReply, e.Command.ArgumentsAsList[1]));
                    return;
                }

                // Check if the user has any artifacts
                if (storage.artifacts.Count == 0)
                {
                    botClient.SendMessage(e.Command.ChatMessage.Channel, string.Format(
                        ArtifactNoneInAccount,
                        e.Command.ArgumentsAsList[1]));
                    return;
                }

                foreach (Artifact art in storage.artifacts)
                {
                    botClient.SendMessage(e.Command.ChatMessage.Channel, Artifact.ToChat(art));
                }
            }

            if (e.Command.ArgumentsAsList.Count == 2 && e.Command.ArgumentsAsList[0].ToLower() == "history") // !artifacts history <ID>
            {
                // If the ID is valid send the artifact
                if (Int32.TryParse(e.Command.ArgumentsAsList[1], out int artID) &&
                    prevArtifacts.TryGetValue(artID, out Artifact art)) 
                {
                    botClient.SendMessage(e.Command.ChatMessage.Channel, Artifact.ToChat(art));
                }
                else
                {
                    // If the ID is invalid/artifact doesn't exist then send an error and exit
                    botClient.SendMessage(e.Command.ChatMessage.Channel, string.Format(
                        ArtifactInvalid,
                        artID));
                    return; 
                }
            }
        }

        private void AddArtifact(ref Dictionary<string, UserStorage> storage, string userID, Artifact newArtifact)
        {
            // Get the user's personal storage
            if (!storage.TryGetValue(userID, out UserStorage personalStorage))
            {
                Console.WriteLine($"Could not find a spice account for user ID: {userID}, failed to add artifact to storage");
                return;
            }

            // Add the new artifact and save to file
            personalStorage.artifacts.Add(newArtifact);
            storage[userID] = personalStorage;
            SaveSpiceStorage(storage);
        }

        private void ResetArtifactCooldown(Object source, ElapsedEventArgs e)
        {
            ArtifactDisplayCooldown = false;
        }
    }
}