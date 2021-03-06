using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MageBall
{
    /// <summary>
    /// Huvudansvarig: Zoe
    /// </summary>
    public class ScoreHandler : NetworkBehaviour
    {


        [SyncVar(hook = nameof(OnRedTeamScoreUpdated))]
        private int redTeamScore;
        [SyncVar(hook = nameof(OnBlueTeamScoreUpdated))]
        private int blueTeamScore;

        public event Action<Team, int> ScoreChanged;

        public int RedTeamScore => redTeamScore;
        public int BlueTeamScore => blueTeamScore;

        public Winner Winner
        {
            get
            {
                Winner winner = Winner.RedTeam;

                if (blueTeamScore > redTeamScore)
                    winner = Winner.BlueTeam;
                else if (blueTeamScore == redTeamScore)
                    winner = Winner.Tie;

                return winner;
            }
        }

        public override void OnStartServer()
        {
            Goal[] goals = FindObjectsOfType<Goal>();

            foreach (Goal goal in goals)
                goal.Score += OnScore;
        }

        private void OnRedTeamScoreUpdated(int oldScore, int newScore) => ScoreChanged?.Invoke(Team.Red, newScore);
        private void OnBlueTeamScoreUpdated(int oldScore, int newScore) => ScoreChanged?.Invoke(Team.Blue, newScore);

        [ServerCallback]
        private void OnDestroy()
        {
            Goal[] goals = FindObjectsOfType<Goal>();

            foreach (Goal goal in goals)
                goal.Score -= OnScore;
        }

        [Server]
        private void OnScore(Team team)
        {
            //If the ball goes in the red team's goal blue team gets a point
            //and vice versa.
            switch (team)
            {
                case Team.Red:
                    blueTeamScore++;
                    break;
                case Team.Blue:
                    redTeamScore++;
                    break;
            }
        }

    }
}