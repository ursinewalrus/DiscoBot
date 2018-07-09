using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Discobot.Objects;
using Newtonsoft.Json;

namespace Discobot.Utilities
{
    class InterjectUtilities
    {

        private static string BaseApiEndpoint = @"https://api.datamuse.com/words?";

        public static List<WordInfo> FormatHaikuInput(string input)
        {
            string[] splitInput = input.Split(' ');
            List<string> inputArr = splitInput.Where(word => word.Length > 2).ToList();
            Random random = new Random();
            //if (inputArr.Length < 3 || random.Next(0, 100)>5) return; //need at least 3 words for haiku, give 1/20 chance of trying
            List<List<WordInfo>> smArr = new List<List<WordInfo>>();


            int maxSyls = 0;
            foreach (string word in inputArr)
            {

                string endpoint = BaseApiEndpoint + "ml=" + word + "&md=s";
                var json = new WebClient().DownloadString(endpoint);
                List<WordInfo> sm = JsonConvert.DeserializeObject<List<WordInfo>>(json);
                if (sm.Count > 0)
                {
                    List<WordInfo> scoreSortedsmArr = sm.OrderByDescending(o => o.numSyllables)
                                                               .ThenBy(o => o.score)
                                                               .Where(o => o.numSyllables < 8)
                                                               .ToList();
                    maxSyls += sm.First().numSyllables;
                    smArr.Add(sm);
                }
            }
            int[] syls = { 5, 7, 5 };

            List<WordInfo> haikuWords;
            int haikuType = random.Next(0, 2);
            if (haikuType == 0)
            {
                haikuWords = BuildHaiku(smArr, new List<WordInfo>(), syls, true);
            }
            else
            {
                haikuWords = BuildHaiku(smArr, new List<WordInfo>(), syls, false);
            }
            return haikuWords;
        }


        //wordIndex -> word in sentance on
        //maxSyls->syls less than to check
        //sylIndex -> what sylable to work on
        public static List<WordInfo> BuildHaiku(List<List<WordInfo>> wordLists, List<WordInfo> pickedWords, int[] syls, bool randomPick, int sylIndex = 0, int wordIndex = 0, int maxSyls = 8)
        {
            Random random = new Random();
            if (wordIndex == wordLists.Count)
            {
                wordIndex = 0;
            }
            List<WordInfo> possibleWords = wordLists[wordIndex].OrderByDescending(o => o.score).Where(o => o.numSyllables <= syls[sylIndex] && o.numSyllables < maxSyls).ToList();
            //litterlly can find no words

            if (possibleWords.Count == 0 && maxSyls == 0)
            {
                var nothing = new List<WordInfo>();
                nothing[0].word = "_null_";
                return nothing;
            }
            else if (possibleWords.Count > 0)//if can find another word that fits
            {
                WordInfo pickSim;
                if (randomPick)
                {
                    int pick = random.Next(0, possibleWords.Count);
                    pickSim = possibleWords.Skip(pick).First();
                }
                else
                {
                    pickSim = possibleWords.First();
                }
                pickedWords.Add(pickSim);
                syls[sylIndex] -= pickSim.numSyllables;
                wordIndex++;

                //next line
                if (syls[sylIndex] == 0)
                {
                    if (sylIndex < 2)
                    {
                        //reset - should only reset on new line?
                        maxSyls = 8;

                        sylIndex++;
                        //REMOVE THESE ON NEWLINES -> move this elsewhere
                        pickedWords.Last().newline = "\r\n";
                        BuildHaiku(wordLists, pickedWords, syls, randomPick, sylIndex, wordIndex, maxSyls);
                    }
                    else //done
                    {
                        return pickedWords;
                    }
                }
                else //same line
                {
                    BuildHaiku(wordLists, pickedWords, syls, randomPick, sylIndex, wordIndex, maxSyls);
                }
            }
            else //backtrack
            {
                wordIndex--;
                WordInfo lastSim = pickedWords.Last();

                if ((sylIndex == 1 && syls[sylIndex] == 7) || (sylIndex == 2 && syls[sylIndex] == 5))
                {
                    sylIndex--;
                    syls[sylIndex] += lastSim.numSyllables;
                    pickedWords.RemoveAt(pickedWords.Count - 1);
                    BuildHaiku(wordLists, pickedWords, syls, randomPick, sylIndex, wordIndex, maxSyls);
                }
                else
                {
                    //backtrack same line
                    maxSyls = lastSim.numSyllables - 1;
                    syls[sylIndex] += lastSim.numSyllables;
                    pickedWords.RemoveAt(pickedWords.Count - 1);
                    BuildHaiku(wordLists, pickedWords, syls, randomPick, sylIndex, wordIndex, maxSyls);

                }
                //}
            }
            return pickedWords;
        }

        //goign to be removed for the fluent api
        public static List<WordInfo> getSentanceWordInfo(List<string> input)
        {
            List<WordInfo> WordInfo = new List<WordInfo>();

            foreach (string word in input)
            {
                string WordInfoQS = BaseApiEndpoint + "sp=" + word + "&md=ps&qe=sp&max=1";

                var WordInfoJson = new WebClient().DownloadString(WordInfoQS);

                WordInfo.AddRange(JsonConvert.DeserializeObject<List<WordInfo>>(WordInfoJson));
            }
            ;
            return WordInfo;
        }

        public static List<WordInfo> getRymingWord(string word)
        {
            string rymeInfoQS = BaseApiEndpoint + "rel_rhy=" + word + "&md=ps";
            var rymeInfoJson = new WebClient().DownloadString(rymeInfoQS);
            var rymInfo =  JsonConvert.DeserializeObject<List<WordInfo>>(rymeInfoJson);
            return rymInfo;
        }

        public static List<WordInfo> getWordMeansLike(string word)
        {
            string endpoint = BaseApiEndpoint + "ml=" + word + "&md=s";
            var WordInfoJson = new WebClient().DownloadString(endpoint);
            return JsonConvert.DeserializeObject<List<WordInfo>>(WordInfoJson);
        }

        public static List<WordInfo> getNextWord(string word)
        {
            string endpoint = BaseApiEndpoint + "ls=" + word;
            var WordInfoJson = new WebClient().DownloadString(endpoint);
            return JsonConvert.DeserializeObject<List<WordInfo>>(WordInfoJson);
        }



    }
}
