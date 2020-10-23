﻿namespace RedSpiceBot
{
    class Artifact
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
        public uint ID { get; set; }
        public ArtifactRarity Rarity { get; set; }
        public int Value { get; set; }

        public static string ToString(Artifact artifact)
        {
            string artifactString = "";

            artifactString += "Artifact: " + artifact.Name + "\n";
            artifactString += "Artifact ID: " + artifact.ID + "\n";
            artifactString += "Artifact Rarity: " + artifact.Rarity + "\n";
            artifactString += "Artifact Value: " + artifact.Value + " Red Spice \n";

            return artifactString;
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
