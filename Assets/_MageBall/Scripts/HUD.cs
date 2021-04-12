using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.UI;

namespace MageBall
{
    public class HUD : NetworkBehaviour
    {
        [Header("UI")]
        [SerializeField] private Text time;
        [SerializeField] private Text blueTeamScoreText;
        [SerializeField] private Text redTeamScoreText;

        public override void OnStartAuthority()
        { 
            ScoreHandler scoreHandler = FindObjectOfType<ScoreHandler>();
            MatchTimer matchTimer = FindObjectOfType<MatchTimer>();

            if (scoreHandler != null)
                scoreHandler.scoreChanged += OnScoreChanged;
            if (matchTimer != null)
                matchTimer.timeChanged += OnTimeChanged;
        }

        [ClientCallback]
        private void OnDestroy()
        {
            ScoreHandler scoreHandler = FindObjectOfType<ScoreHandler>();
            MatchTimer matchTimer = FindObjectOfType<MatchTimer>();

            if (scoreHandler != null)
                scoreHandler.scoreChanged -= OnScoreChanged;
            if (matchTimer != null)
                matchTimer.timeChanged -= OnTimeChanged;
        }

        private void OnTimeChanged(int minutes, int seconds)
        {
            if (seconds >= 10)
                time.text = minutes + ":" + seconds;
            else
                time.text = minutes + ":0" + seconds;
        }

        private void OnScoreChanged(Team team, int newScore)
        {
            switch (team)
            {
                case Team.Red:
                    redTeamScoreText.text = newScore.ToString();
                    break;
                case Team.Blue:
                    blueTeamScoreText.text = newScore.ToString();
                    break;
            }
        }
    }
}