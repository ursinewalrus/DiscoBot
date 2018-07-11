using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Discobot.Objects
{

    public static class DataMuseQsArgs
    {
        public static class ConstraintArgments
        {
            public static string MeansLike = "ml";
            public static string LeftContext = "lc";
            public static string RightContext = "rc";
            public static string SoundsLike = "sl";
            public static string Self = "qe";

        }

        public static class MetaDataTags
        {
            public static string PartsOfSpeach = "p";
            public static string Syllables = "s";
            public static string Frequency = "f";

        }

        public static class Relations
        {
            public static string NounsModByAdj = "jja";
            public static string AdjModByNoun = "jjb";
            public static string Synonyms = "syn";
            public static string AssocWith = "trg";
            public static string Anyonyms = "ant";
            public static string Followers = "bga";
            public static string Predecessors = "bgb";
            public static string Rymes = "rhy";
            public static string Homophones = "hom";
        }
    }
    public enum PoS
    {
        v = 0,//verb
        n = 1,//noun
        adj = 2,//adjetive
        adv = 3,//adverb
        u = 4//undefined
    }
}
