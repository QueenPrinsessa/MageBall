using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.UI;
using TMPro;

namespace MageBall
{
    public class HUD : NetworkBehaviour
    {
        [Header("UI")]
        [SerializeField] private TMP_Text time;
        [SerializeField] private TMP_Text blueTeamScoreText;
        [SerializeField] private TMP_Text redTeamScoreText;
        [SerializeField] private GameObject goalScoredUI;
        [SerializeField] private GameObject matchEndUI;

        private NetworkManagerMageBall networkManager;

        private NetworkManagerMageBall NetworkManager
        {
            get
            {
                if (networkManager != null)
                    return networkManager;

                return networkManager = Mirror.NetworkManager.singleton as NetworkManagerMageBall;
            }
        }

        public override void OnStartAuthority()
        { 
            ScoreHandler scoreHandler = FindObjectOfType<ScoreHandler>();
            MatchTimer matchTimer = FindObjectOfType<MatchTimer>();

            if (scoreHandler != null)
                scoreHandler.scoreChanged += OnScoreChanged;

            if (matchTimer != null)
            {
                matchTimer.timeChanged += OnTimeChanged;
                matchTimer.matchEnd += OnMatchEnd;
            }
        }

        [ClientCallback]
        private void OnDestroy()
        {
            ScoreHandler scoreHandler = FindObjectOfType<ScoreHandler>();
            MatchTimer matchTimer = FindObjectOfType<MatchTimer>();

            if (scoreHandler != null)
                scoreHandler.scoreChanged -= OnScoreChanged;

            if (matchTimer != null)
            {
                matchTimer.timeChanged -= OnTimeChanged;
                matchTimer.matchEnd -= OnMatchEnd;
            }
        }

        private void OnMatchEnd()
        {
            Debug.Log("On match end");

            if (matchEndUI == null)
                return;

            Debug.Log("scorehandler");

            ScoreHandler scoreHandler = FindObjectOfType<ScoreHandler>();

            if (scoreHandler == null)
            {
                Debug.LogError("There is no ScoreHandler in the current scene. Did you forget to add one?");
                return;
            }

            Debug.Log("match end UI");

            matchEndUI.SetActive(true);

            TMP_Text matchEndText = matchEndUI.GetComponentInChildren<TMP_Text>();

            if (matchEndText == null)
            {
                Debug.LogError("There is no match end text in the match end UI!");
                return;
            }

            switch (scoreHandler.Winner)
            {
                case Winner.RedTeam:
                    matchEndText.text = "<color=\"red\">RED</color> TEAM WINS!";
                    break;
                case Winner.BlueTeam:
                    matchEndText.text = "<color=\"blue\">BLUE</color> TEAM WINS!";
                    break;
                case Winner.Tie:
                    matchEndText.text = "IT'S A TIE!";
                    break;
            }
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
            goalScoredUI.SetActive(true);
            TMP_Text goalScoredText = goalScoredUI.GetComponentInChildren<TMP_Text>();
            if (goalScoredText != null)
                goalScoredText.text = $"<color=\"{team.ToString().ToLower()}\">{team.ToString().ToUpper()}</color> TEAM SCORES";
            StartCoroutine(DisableGoalScoredUI());

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

        private IEnumerator DisableGoalScoredUI()
        {
            yield return new WaitForSeconds(NetworkManager.WaitBeforeResetAfterGoalInSeconds);
            goalScoredUI.SetActive(false);
        }
    }
}