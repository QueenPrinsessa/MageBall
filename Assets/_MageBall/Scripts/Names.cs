using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MageBall
{
    public static class Names 
    {
        public static readonly string RedTeamName = "The Defiant";
        public static readonly string BlueTeamName = "The Templar Sages";

        public static string NameFromTeam(Team team)
        {
            switch (team)
            {
                case Team.Red:
                    return RedTeamName;
                case Team.Blue:
                    return BlueTeamName;
                default:
                    return team.ToString();
            }
        }
    }
}