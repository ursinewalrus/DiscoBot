using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

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
        public List<string> tags { get; set; }
        public string newline = "";

        public WordInfo(string word, int score, int numSyllables, List<string> tags)
        {
            this.word = word;
            this.score = score;
            this.numSyllables = numSyllables;
            this.tags = tags;
        }
    }

    //probably not needed but possibly more extensible later
    public class QueryStringBuilder : ICanConstructQuery, ICanDoRun
    {

        private List<string> QsConstraintData { get; set;  }
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
        public QueryStringBuilder(string word)
        {
            this.Word = word;
            this.QsConstraintData = new List<string>();
            this.QsMetaData = new List<string>();
            this.QsRelated = "";
            this.QueryString = BaseApiEndpoint;
        }

        public static ICanConstructQuery AddSearchWord(string word)
        {
            return new QueryStringBuilder(word);
        }

        public ICanConstructQuery AddConstraint(string constraint)
        {
            this.QsConstraintData.Add(constraint);
            return this;
        }

        public ICanConstructQuery AddMetaTag(string metaTag)
        {
            this.QsMetaData.Add(metaTag);
            return this;
        }

        public ICanConstructQuery AddRelationType(string relationType)
        {
            this.QsRelated = relationType;
            return this;
        }

        public ICanDoRun ConstructQuery()
        {
            if (this.QsConstraintData.Contains(DataMuseQsArgs.ConstraintArgments.Self))
            {
                this.QueryString += "sp=" + this.Word + "&qe=sp&max=1&";
                this.QsConstraintData.Remove(DataMuseQsArgs.ConstraintArgments.Self);
            }
            this.QueryString += (this.QsConstraintData.Count() > 0) ? String.Join("=" + this.Word, this.QsConstraintData) + "=" + this.Word + "&" : "";
            this.QueryString += (this.QsMetaData.Count() > 0) ? "md="+ String.Join("",this.QsMetaData) + "&" : "";
            this.QueryString += (this.QsRelated != "") ? "rel_" + this.QsRelated + "=" + this.Word : "";
            return this;
        }
        //what why clearly missing something
        List<WordInfo> ICanDoRun.RunQuery()
        {
            var json = new WebClient().DownloadString(this.QueryString);
            List<WordInfo> queryrResults = JsonConvert.DeserializeObject<List<WordInfo>>(json);
            return queryrResults;
        }
    }

 
   public interface ICanConstructQuery
    {
        ICanConstructQuery AddConstraint(string constraints);
        ICanConstructQuery AddMetaTag(string metaTags);
        ICanConstructQuery AddRelationType(string relation);
        ICanDoRun ConstructQuery();

    }

    public interface ICanDoRun
    {
        List<WordInfo> RunQuery();
    }

}
