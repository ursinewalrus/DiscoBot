using System;
using System.Collections.Generic;
using System.Text;

namespace Discobot.Objects
{
    class InterjectObjects
    {
    }
    //for Haikus
    public class SimilarMeanings
    {
        public string word { get; set; }
        public int score { get; set; }
        public int numSyllables { get; set; }
        public string[] tags { get; set; }
        public string newline = "";

        public SimilarMeanings(string word, int score, int numSyllables, string[] tags)
        {
            this.word = word;
            this.score = score;
            this.numSyllables = numSyllables;
            this.tags = tags;
        }
    }

}
