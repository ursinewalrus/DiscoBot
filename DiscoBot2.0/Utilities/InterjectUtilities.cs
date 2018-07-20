using System;
using System.Collections.Generic;
using System.Linq;
using Discobot.Objects;

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

                List<WordInfo> sm = QueryStringBuilder.AddSearchWord(word)
                                  .AddConstraint(DataMuseQsArgs.ConstraintArgments.MeansLike)
                                  .AddMetaTag(DataMuseQsArgs.MetaDataTags.Syllables)
                                  .ConstructQuery()
                                  .RunQuery();
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


        // Limrick
        // 7-10
        // 7-10
        // 7-10
        // 5-7
        // 5-7
        // 7-10
        public static string BuildLimrick(List<string> words)
        {
            List<WordInfo> wordsInfo = (from word in words
                                        select QueryStringBuilder.AddSearchWord(word)
                                                      .AddConstraint(DataMuseQsArgs.ConstraintArgments.Self)
                                                      .AddMetaTag(DataMuseQsArgs.MetaDataTags.Syllables)
                                                      .AddMetaTag(DataMuseQsArgs.MetaDataTags.PartsOfSpeach)
                                                      .ConstructQuery()
                                                      .RunQuery()[0]).ToList();
            int totalSyls = Math.Max(Math.Min(10, wordsInfo.Sum(w => w.numSyllables)), 7); //makes sure its between 7-10 syls
            //yoink this kind of thing out
            //looking for nouns
            List<WordInfo> nextLineInspirations = wordsInfo.Where(w => w.tags.Contains("n")).ToList();
            //otherwise just get the word with the most sylables
            if (nextLineInspirations.Count() < 1)
            {
                nextLineInspirations.Add(wordsInfo.OrderByDescending(w => w.numSyllables).First());
            }

            //maybe build them backwards from the rymes?
            string ARyme = words[words.Count() - 1];
            List<WordInfo> ARymes = QueryStringBuilder.AddSearchWord(ARyme)
                                        .AddRelationType(DataMuseQsArgs.Relations.Rymes)
                                        .AddMetaTag(DataMuseQsArgs.MetaDataTags.Syllables)
                                        .AddMetaTag(DataMuseQsArgs.MetaDataTags.PartsOfSpeach)
                                        .ConstructQuery()
                                        .RunQuery()
                                        .ToList();
            if (ARymes.Count() == 0)
            {
                ARymes.AddRange(

                    QueryStringBuilder.AddSearchWord(ARyme)
                                        .AddConstraint(DataMuseQsArgs.ConstraintArgments.MeansLike)
                                        .AddMetaTag(DataMuseQsArgs.MetaDataTags.Syllables)
                                        .AddMetaTag(DataMuseQsArgs.MetaDataTags.PartsOfSpeach)
                                        .ConstructQuery()
                                        .RunQuery()
                                        .ToList()
                    );
            }
            
            //A rymes
            List<List<WordInfo>> ALines = new List<List<WordInfo>>();
            for (var i = 0; i < 3; i++)
            {
                ALines.Add(BuildLimrickline(totalSyls, ARymes));
            }
            //B rymes
            //seed for B rymes
            List<WordInfo> BLineSeed = GetAssocWordByPos(ALines[1].Last());
            List<List<WordInfo>> BLines = new List<List<WordInfo>>();
            for (var i = 0; i<2; i++)
            {
                BLines.Add(BuildLimrickline(totalSyls-3, BLineSeed));
            }
            string completedLimrick = "";
            completedLimrick += String.Join(" ", ALines[0].Select(s => s.word).ToList()) + "\n";
            completedLimrick += String.Join(" ", ALines[1].Select(s => s.word).ToList()) + "\n";
            completedLimrick += String.Join(" ", BLines[0].Select(s => s.word).ToList()) + "\n";
            completedLimrick += String.Join(" ", BLines[1].Select(s => s.word).ToList()) + "\n";
            completedLimrick += String.Join(" ", ALines[2].Select(s => s.word).ToList()) + "\n";
            return completedLimrick;

        }

        private static List<WordInfo> BuildLimrickline(int totalSyls, List<WordInfo> Rymes)
        {
            Random rand = new Random();
            WordInfo lineEnd = Rymes[rand.Next(0, Rymes.Count())];
            WordInfo currentWord = lineEnd;
            int sylsLeftForLine = totalSyls;
            List<WordInfo> limrickLine = new List<WordInfo>() ;
            while (sylsLeftForLine > 0)
            {
                limrickLine.Insert(0, currentWord);
                sylsLeftForLine -= currentWord.numSyllables;
                currentWord = GetWordToLeft(currentWord);
            }
            return limrickLine;
        }

        public static WordInfo GetWordToLeft(WordInfo word)
        {
            //try catch mebe
            List<WordInfo> leftWords = QueryStringBuilder.AddSearchWord(word.word)
                                    .AddRelationType(DataMuseQsArgs.Relations.Predecessors)
                                    .AddMetaTag(DataMuseQsArgs.MetaDataTags.Syllables)
                                    .AddMetaTag(DataMuseQsArgs.MetaDataTags.PartsOfSpeach)
                                    .ConstructQuery()
                                    .RunQuery();
            if(leftWords.Count() < 1)
            {
                leftWords.AddRange(

                     QueryStringBuilder.AddSearchWord(word.word)
                                    .AddConstraint(DataMuseQsArgs.ConstraintArgments.MeansLike)
                                    .AddMetaTag(DataMuseQsArgs.MetaDataTags.Syllables)
                                    .AddMetaTag(DataMuseQsArgs.MetaDataTags.PartsOfSpeach)
                                    .ConstructQuery()
                                    .RunQuery()

                    );
            }
            if(leftWords.Count() == 0)
            {
                leftWords.Add(word);
            }
            
            return GetLongestWord(leftWords.ToList());
        }

        public static WordInfo GetLongestWord(List<WordInfo> words)
        {
            return words.OrderByDescending(w => w.numSyllables).ToList().First();
        }

        public static WordInfo ExtractByPoS(List<WordInfo> words, string pos)
        {
            Random rand = new Random();
            var nouns = words.Where(w => w.tags.Contains(pos)).ToList();
            int choice = rand.Next(0, nouns.Count());
            return nouns[choice];
        }

        public static List<WordInfo> GetAssocWordByPos(WordInfo word)
        {
            string posToSearch = "";
            posToSearch = (word.tags.Contains("n")) ? DataMuseQsArgs.Relations.AdjModByNoun :

                          (word.tags.Contains("adv")) ? DataMuseQsArgs.Relations.NounsModByAdj : DataMuseQsArgs.Relations.Anyonyms;


            List<WordInfo> assocWords = QueryStringBuilder.AddSearchWord(word.word)
                                    .AddRelationType(posToSearch)
                                    .AddMetaTag(DataMuseQsArgs.MetaDataTags.Syllables)
                                    .AddMetaTag(DataMuseQsArgs.MetaDataTags.PartsOfSpeach)
                                    .ConstructQuery()
                                    .RunQuery();
            if(assocWords.Count() < 1)
            {
                assocWords.AddRange(

                    QueryStringBuilder.AddSearchWord(word.word)
                                    .AddConstraint(DataMuseQsArgs.ConstraintArgments.MeansLike)
                                    .AddMetaTag(DataMuseQsArgs.MetaDataTags.Syllables)
                                    .AddMetaTag(DataMuseQsArgs.MetaDataTags.PartsOfSpeach)
                                    .ConstructQuery()
                                    .RunQuery()

                    );
            }
            return assocWords;
        }
    }


}
