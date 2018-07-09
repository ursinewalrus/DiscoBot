using System;
using System.Collections.Generic;
using System.Text;

namespace Discobot.Objects
{
    class InterjectObjects
    {
    }
    //for Haikus
    public class WordInfo
    {
        public string word { get; set; }
        public int score { get; set; }
        public int numSyllables { get; set; }
        public string[] tags { get; set; }
        public string newline = "";

        public WordInfo(string word, int score, int numSyllables, string[] tags)
        {
            this.word = word;
            this.score = score;
            this.numSyllables = numSyllables;
            this.tags = tags;
        }
    }

    //probably not needed but possibly more extensible later
    public class QueryStringInfo : ICanSetConstraintData, ICanSetMetaData, ICanSetRelatedWord, ICanDoRun
    {

        private static Dictionary<string, string> QsConstraintOptions = new Dictionary<string, string>
        {
            {"meansLike", "ml" },
            {"leftContext", "lc" },
            {"rightContext", "rc" },
            {"soundsLike", "sl" },
            {"self","qe=" }

        };

        private static Dictionary<string, string> QsMetaDataOptions = new Dictionary<string, string>
        {
            {"partsOfSpeech", "p" },
            {"syllables", "s" },
            {"frequency", "f" }
        };
        //rel_
        private static Dictionary<string, string> QsRelatedOptions = new Dictionary<string, string>
        {
            {"nounsModByAdj", "jja" },
            {"adjModByNoun", "jjb" },
            {"synonyms", "syn" },
            {"assocWith", "trg" },
            {"anyonyms", "ant" },
            {"followers", "bga" },
            {"predecessors", "bgb" },
            {"rymes", "rhy" },
            {"homophones", "hom" },
        };
        private List<string> QsConstraintData;
        public List<string> QsMetaData { get; set; }
        public string QsRelated { get; set; }
        public string Word { get; set; }
        public string QueryString { get; set; }

        public string BaseApiEndpoint = @"https://api.datamuse.com/words?";
        /// <summary>
        /// Construct and process a querystring to hit the datamuse api
        /// </summary>
        /// <param name="QsConstraintData">Constraint options, uses keys from QsConstraintOptions</param>
        /// <param name="QsMetaData">General things to return with each word, use keys in QsMetaDataOptions</param>
        /// <param name="QsRelated">Only can use one of these, get related words, uses keys from QsRelatedOptions </param>
        public QueryStringInfo(/*List<string> QsConstraintData, List<string> QsMetaData, string QsRelated,*/ string word)
        {
            //this.QsConstraintData = QsConstraintData;
            //this.QsMetaData = QsMetaData;
            //this.QsRelated = QsRelated;
            this.Word = word;
        }

        public static ICanSetConstraintData AddSearchWord(string word)
        {
            return new QueryStringInfo(word);
        }

        public ICanSetMetaData AddConstraintData(List<string> constraints)
        {
            this.QsConstraintData = constraints;
            return this;
        }

        public ICanSetRelatedWord AddMetaData(List<string> metaTags)
        {
            this.QsMetaData = metaTags;
            return this;
        }

        public QueryStringInfo AddRelationType(string relationType)
        {
            this.QsRelated = relationType;
            return this;
        }

        public ICanDoRun ConstructQuery()
        {
            this.QueryString = "build this";
            return this;
        }
        //what why clearly missing something
        List<WordInfo> ICanDoRun.RunQuery()
        {
            return new List<WordInfo>();
        }
    }

 
    public interface ICanSetConstraintData
    {
        ICanSetRelatedWord AddMetaData(List<string> constraints);
        ICanSetMetaData AddConstraintData(List<string> constraints);
    }

    public interface ICanSetMetaData
    {
        ICanSetRelatedWord AddMetaData(List<string> metaTags);
    }

    public interface ICanSetRelatedWord
    {
        QueryStringInfo AddRelationType(string word);
    }

    public interface ICanDoRun
    {
        List<WordInfo> RunQuery();
    }
    //fluent experiment  https://scottlilly.com/how-to-create-a-fluent-interface-in-c/

   // var thing = QueryStringInfo.AddSearchWord("word").AddConstraintData(new List<string>() { "A", "A" }).AddMetaData(new List<string>() { "A" }).AddRelationType("A").ConstructQuery().RunQuery();
}
