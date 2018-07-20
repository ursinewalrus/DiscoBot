using System.Collections.Generic;

namespace Discobot.Objects
{
    class OWLPlayer
    {
        public string id { get; set; }
        public string handle { get; set; }
        public string name { get; set; }
        public string homeLocation { get; set; }
        public string familyName { get; set; }
        public string givenName { get; set; }
        public string nationality { get; set; }
        public string headshot { get; set; }
        public List<OWLTeam> teams {get; set;}
        public OWLPlayerAttributes attributes { get; set; }
    }

    class OWLTeam
    {
        public string handle { get; set; }
        public string name { get; set; }
        public string homeLocation { get; set; }
    }

    class OWLPlayerAttributes
    {
        public string role { get; set; }
        public List<string> heroes { get; set; }
        public string player_number { get; set; }

    }

}
