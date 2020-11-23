using RedSpiceBot.ArtifactGenerator;
using System;
using System.Collections.Generic;
using System.IO;
using WebSocketSharp;

namespace RedSpiceBot
{
    static class ArtifactParser
    {
        

        public static List<Artifact> ParseArtifacts(List<string> artifacts)
        {
            List<string> parsedArtifacts = new List<string>();

            foreach (string curArtifact in artifacts)
            {
                string newArtifact = BuildString(curArtifact);
                if (newArtifact != null) { parsedArtifacts.Add(newArtifact); }
            }

            List<Artifact> finishedArtifacts = BuildArtifacts(parsedArtifacts);

            return finishedArtifacts;
        }

        private static string BuildString(string curArtifact)
        {
            string newArtifact = "";
            string artifact = curArtifact.TrimEnd('o', 't', 'd'); // Trailing articles should be cut off
            artifact = artifact.TrimStart('o', 'd'); // Don't start with "Of" or "And"

            // If it's a dud return null
            if (artifact.IsNullOrEmpty()) { return null; }

            foreach (char character in artifact)
            {
                switch (character)
                {
                    /*
                    % Uses a character token system to determine string structure
                    % n = Name (generated randomly)
                    % s = Name's (generated randomly)
                    % u = Noun (randomly chosen from a list)
                    % a = Adjective (randomly chosen from a list)
                    % o = "of"
                    % t = "the"
                    */

                    case 'n':
                        newArtifact += "<NAME>";
                        break;

                    case 's':
                        newArtifact += "<NAME>'s";
                        break;

                    case 'u':
                        newArtifact += "<NOUN>";
                        break;

                    case 'a':
                        newArtifact += "<ADJECTIVE>";
                        break;

                    case 'o':
                        newArtifact += "of";
                        break;

                    case 't':
                        newArtifact += "the";
                        break;

                    case 'd':
                        newArtifact += "and";
                        break;

                    default:
                        break;
                }

                newArtifact += " "; // Add a space after each step, we'll trim at the end to remove trailing spaces 
            }

            // Clean up the artifact
            newArtifact = newArtifact.Trim();
            newArtifact = Char.ToUpper(newArtifact[0]) + newArtifact.Substring(1);

            newArtifact = FillString(newArtifact);

            return newArtifact;
        }

        private static string FillString(string rawArtifact)
        {
            Random rnd = new Random((int)DateTime.Now.Ticks & 0x0000FFFF);

            // Set up the name generator
            MarkovChainsNameGenerator generator = new MarkovChainsNameGenerator(random: rnd, minLength: 3, maxLength: 7);;
            generator.TrainMapBuilder(@"../../ArtifactGenerator/Sources/names.txt");

            int index;
            string line;
            string curArtifact = rawArtifact;
            List<string> nouns = new List<string>();
            List<string> adjectives = new List<string>();

            // Load the lists of nouns and adjectives
            using (StreamReader r = new StreamReader(@"../../ArtifactGenerator/Sources/nouns.txt"))
            {
                while ((line = r.ReadLine()) != null)
                {
                    nouns.Add(line);
                }
            }
            using (StreamReader r = new StreamReader(@"../../ArtifactGenerator/Sources/adjectives.txt"))
            {
                while ((line = r.ReadLine()) != null)
                {
                    adjectives.Add(line);
                }
            }

            // Replace all instance of <NAME> with a generated name
            while ((index = curArtifact.IndexOf("<NAME>")) != -1)
            {
                curArtifact = curArtifact.Remove(index, "<NAME>".Length);
                curArtifact = curArtifact.Insert(index, generator.GetName());
            }

            // Replace all instance of <NOUN> with a randomly chosen noun
            while ((index = curArtifact.IndexOf("<NOUN>")) != -1)
            {
                curArtifact = curArtifact.Remove(index, "<NOUN>".Length);
                curArtifact = curArtifact.Insert(index, nouns[rnd.Next(0, nouns.Count)]);
            }

            // Replace all instance of <ADJECTIVE> with a randomly chosen noun
            while ((index = curArtifact.IndexOf("<ADJECTIVE>")) != -1)
            {
                curArtifact = curArtifact.Remove(index, "<ADJECTIVE>".Length);
                curArtifact = curArtifact.Insert(index, adjectives[rnd.Next(0, adjectives.Count)]);
            }

            return curArtifact;
        }

    private static List<Artifact> BuildArtifacts(List<string> parsedArtifacts)
        {
            List<Artifact> finishedArtifacts = new List<Artifact>();
            Random rnd = new Random((int)DateTime.Now.Ticks & 0x0000FFFF);

            int curMundane = 0;
            int curCommon = 0;
            int curUncommon = 0;
            int curRare = 0;
            int curEpic = 0;
            int curLegendary = 0;

            // Get items of each rarity, up to the max
            foreach (string artifact in parsedArtifacts)
            {
                if (curMundane < Artifact.MundaneArtifactsMax && 
                    artifact.Split(' ').Length >= Artifact.MundaneMinWords &&
                    artifact.Split(' ').Length <= Artifact.MundaneMaxWords)
                {
                    finishedArtifacts.Add(BuildArtifact(artifact, ArtifactRarity.Mundane, rnd));
                    curMundane += 1;
                }
                if (curCommon < Artifact.CommonArtifactsMax && 
                    artifact.Split(' ').Length >= Artifact.CommonMinWords &&
                    artifact.Split(' ').Length <= Artifact.CommonMaxWords)
                {
                    finishedArtifacts.Add(BuildArtifact(artifact, ArtifactRarity.Common, rnd));
                    curCommon += 1;
                }
                if (curUncommon < Artifact.UncommonArtifactsMax &&
                    artifact.Split(' ').Length >= Artifact.UncommonMinWords &&
                    artifact.Split(' ').Length <= Artifact.UncommonMaxWords)
                {
                    finishedArtifacts.Add(BuildArtifact(artifact, ArtifactRarity.Uncommon, rnd));
                    curUncommon += 1;
                }
                if (curRare < Artifact.RareArtifactsMax &&
                    artifact.Split(' ').Length >= Artifact.RareMinWords &&
                    artifact.Split(' ').Length <= Artifact.RareMaxWords)
                {
                    finishedArtifacts.Add(BuildArtifact(artifact, ArtifactRarity.Rare, rnd));
                    curRare += 1;
                }
                if (curEpic < Artifact.EpicArtifactsMax &&
                    artifact.Split(' ').Length >= Artifact.EpicMinWords &&
                    artifact.Split(' ').Length <= Artifact.EpicMaxWords)
                {
                    finishedArtifacts.Add(BuildArtifact(artifact, ArtifactRarity.Epic, rnd));
                    curEpic += 1;
                }
                if (curLegendary < Artifact.LegendaryArtifactsMax &&
                    artifact.Split(' ').Length >= Artifact.LegendaryMinWords &&
                    artifact.Split(' ').Length <= Artifact.LegendaryMaxWords)
                {
                    finishedArtifacts.Add(BuildArtifact(artifact, ArtifactRarity.Legendary, rnd));
                    curLegendary += 1;
                }
            }

            return finishedArtifacts;
        }

        private static Artifact BuildArtifact(string artifact, ArtifactRarity rarity, Random rnd)
        {
            Artifact finishedArtifact = new Artifact();
            int min, max;

            switch (rarity)
            {
                case ArtifactRarity.Mundane:
                    min = Artifact.MundaneMinPrice;
                    max = Artifact.MundaneMaxPrice;
                    break;

                case ArtifactRarity.Common:
                    min = Artifact.CommonMinPrice;
                    max = Artifact.CommonMaxPrice;
                    break;

                case ArtifactRarity.Uncommon:
                    min = Artifact.UncommonMinPrice;
                    max = Artifact.UncommonMaxPrice;
                    break;

                case ArtifactRarity.Rare:
                    min = Artifact.RareMinPrice;
                    max = Artifact.RareMaxPrice;
                    break;

                case ArtifactRarity.Epic:
                    min = Artifact.EpicMinPrice;
                    max = Artifact.EpicMaxPrice;
                    break;

                case ArtifactRarity.Legendary:
                    min = Artifact.LegendaryMinPrice;
                    max = Artifact.LegendaryMaxPrice;
                    break;

                default:
                    min = 0;
                    max = 0;
                    break;
            }

            finishedArtifact.Name = artifact;
            finishedArtifact.Rarity = rarity;
            finishedArtifact.ID = 0;
            finishedArtifact.Value = rnd.Next(min, max + 1);

            return finishedArtifact;
        }
    }
}