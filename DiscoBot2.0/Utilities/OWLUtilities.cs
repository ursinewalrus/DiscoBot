using System;
using System.Collections.Generic;
using System.Linq;
using Discobot.Objects;
using Newtonsoft.Json.Linq;

namespace Discobot.Utilities
{
    class OWLUtilities
    {
        public static List<string> ParseTeamInfo(JToken allInfo, string image)
        {
            List<string> results = new List<string>();

            JToken teams = allInfo.SelectToken("content");
            var teamList = teams.ToList();
            foreach (JToken team in teamList)
            {
                string placement = team.SelectToken("placement").ToString();

                JToken teamInfo = team.SelectToken("competitor");
                string name = teamInfo.SelectToken("name").ToString();
                string logo = "";
                if (image == "im")
                {
                    logo = teamInfo.SelectToken("logo").ToString();
                }
                JToken allRecords = team.SelectToken("records").ToArray()[0];

                string wins = allRecords.SelectToken("matchWin").ToString();
                string losses = allRecords.SelectToken("matchLoss").ToString();
                string draws = allRecords.SelectToken("matchDraw").ToString();

                results.Add(name + " | " + "Placement: " + placement + " | " + "W/L/D: " + wins + "/" + losses + "/" + draws + "\r\n" + logo);

            }
            return results;
        }

        public static string GetPlayerData(JToken allInfo, string playerName)
        {

            JToken playersJson = allInfo.SelectToken("content");

            List<OWLPlayer> playerJsonList = new List<OWLPlayer>();

            foreach (JToken player in playersJson)
            {
                List<JToken> Jteams = player.SelectToken("teams").ToList();
                List<OWLTeam> teams = new List<OWLTeam>();
                foreach (JToken jteam in Jteams)
                {
                    teams.Add(jteam.SelectToken("team").ToObject<OWLTeam>());
                }
                OWLPlayer playerObj = player.ToObject<OWLPlayer>();

                //JToken heros = player.SelectToken("attributes").SelectToken("heros");
                //;
                playerObj.teams = teams;
                playerJsonList.Add(playerObj);
            }
            if (playerName != "default")
            {
                OWLPlayer player = playerJsonList.Where(c => c.name == playerName).First();
                string playerInfo = player.givenName + " "
                                  + player.familyName
                                  + " AKA " + player.name
                                  + " of " + player.homeLocation + "," + player.nationality
                                  + " on team " + player.teams[0].name
                                  + " plays " + player.attributes.role + " as " + String.Join(", ", player.attributes.heroes)
                                  + "\n\r" + player.headshot;
                return playerInfo;
            }
            else
            {
                return "ask for a specific player";
            }

        }
    }
}
