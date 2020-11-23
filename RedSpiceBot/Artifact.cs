using Newtonsoft.Json;
using RedSpiceBot.ArtifactGenerator;
using System;
using System.Collections.Generic;
using System.IO;

namespace RedSpiceBot
{
    public class Artifact
    {
        // Rarirty distributions for the shop
        public const int MundaneArtifactsMax = 1;
        public const int CommonArtifactsMax = 3;
        public const int UncommonArtifactsMax = 3;
        public const int RareArtifactsMax = 2;
        public const int EpicArtifactsMax = 1;
        public const int LegendaryArtifactsMax = 1;

        // Price Ranges for rarities (inclusive)
        public const int MundaneMinPrice = 1;
        public const int MundaneMaxPrice = 5;
        public const int CommonMinPrice = 6;
        public const int CommonMaxPrice = 10;
        public const int UncommonMinPrice = 11;
        public const int UncommonMaxPrice = 20;
        public const int RareMinPrice = 21;
        public const int RareMaxPrice = 50;
        public const int EpicMinPrice = 51;
        public const int EpicMaxPrice = 75;
        public const int LegendaryMinPrice = 76;
        public const int LegendaryMaxPrice = 100;

        // Word Ranges for rarities (inclusive)
        public const int MundaneMinWords = 1;
        public const int MundaneMaxWords = 1;
        public const int CommonMinWords = 2;
        public const int CommonMaxWords = 2;
        public const int UncommonMinWords = 3;
        public const int UncommonMaxWords = 4;
        public const int RareMinWords = 5;
        public const int RareMaxWords = 6;
        public const int EpicMinWords = 7;
        public const int EpicMaxWords = 8;
        public const int LegendaryMinWords = 9;
        public const int LegendaryMaxWords = 10;

        public string Name { get; set; }
        public int ID { get; set; }
        public ArtifactRarity Rarity { get; set; }
        public int Value { get; set; }

        public static string ToString(Artifact artifact)
        {
            string artifactString = "Artifact: " + artifact.Name + "\n" +
                "Artifact ID: " + artifact.ID + "\n" +
                "Artifact Rarity: " + artifact.Rarity + "\n" +
                "Artifact Value: " + artifact.Value + " Red Spice \n";

            return artifactString;
        }

        public static string ToChat(Artifact artifact)
        {
            string artifactString = artifact.Name + " is a " +
                artifact.Rarity + " artifact worth " +
                artifact.Value + " Red Spice.";

            return artifactString;
        }

        public static List<Artifact> GenerateArticats(out Dictionary<int, Artifact> artifactHistory)
        {
            // Set up the artifact generator
            Random rnd = new Random((int)DateTime.Now.Ticks & 0x0000FFFF);
            MarkovChainsNameGenerator artifactGenerator = new MarkovChainsNameGenerator(random: rnd, minLength: 2, maxLength: 10, capitalize: false, skipWhitespace: false);
            artifactGenerator.TrainMapBuilder(@"../../ArtifactGenerator/Sources/structures.txt");

            // Get the history of previous artifacts and set up IDs for new artifacts
            artifactHistory = LoadHistory();
            int curID = 0; // By default set the current ID as 0
            if (artifactHistory != null) { curID = artifactHistory.Count; } // If there is a history, then start ID off of that
            else { artifactHistory = new Dictionary<int, Artifact>(); }

            // Get a bunch of artifact strings and send them to the parser
            IEnumerable<string> artifacts = artifactGenerator.GetNames(100); // Generate a bunch of artifacts, the parser will trim it down
            List<Artifact> newArtifacts = ArtifactParser.ParseArtifacts(new List<string>(artifacts));

            // ID the artifacts and save the new artifacts in the history structure
            Console.WriteLine("Todays artifacts:");
            for (int i = 0; i < newArtifacts.Count; i++)
            {
                newArtifacts[i].ID = curID + i + 1;
                artifactHistory[newArtifacts[i].ID] = newArtifacts[i];
                Console.WriteLine(Artifact.ToString(newArtifacts[i]));
            }
            SaveHistory(artifactHistory);

            return newArtifacts;
        }

        private static Dictionary<int, Artifact> LoadHistory()
        {
            using (StreamReader r = new StreamReader(@"../../SpiceStorage/artifactHistory.json"))
            {
                string history = r.ReadToEnd();
                r.Close();
                return JsonConvert.DeserializeObject<Dictionary<int, Artifact>>(history);
            }
        }

        public static void SaveHistory(Dictionary<int, Artifact> history)
        {
            File.WriteAllText(@"../../SpiceStorage/artifactHistory.json", JsonConvert.SerializeObject(history));
        }
    }

    public enum ArtifactRarity
    {
        Mundane, // 1 noun, 1-5 spice
        Common, // 2 words 6-10 spice
        Uncommon, // 3-4 words, 11-20 spice
        Rare, // 5-6 words, 21-50 spice
        Epic, // 7-8 words, 51-75
        Legendary // 9-10 words, 76-100 spice
    }
}
