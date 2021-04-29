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
        [SerializeField] private TMP_Text timeText;
        [SerializeField] private TMP_Text blueTeamScoreText;
        [SerializeField] private TMP_Text redTeamScoreText;
        [SerializeField] private GameObject goalScoredUI;
        [SerializeField] private GameObject matchEndUI;
        [SerializeField] private RawImage barRawImage;
        [SerializeField] private Mask barMask;
        [SerializeField] private Spellcasting spellcasting;

        private float barMaskWidth;
        private Coroutine updateManaBarCoroutine;

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


            updateManaBarCoroutine = StartCoroutine(UpdateManaBar());
            barMaskWidth = barRawImage.rectTransform.rect.width;

            if (barRawImage == null)
                Debug.LogError("could not find raw image");
            
            if (barMask == null)
                Debug.LogError("could not find mask");
            

            if (scoreHandler != null)
                scoreHandler.scoreChanged += OnScoreChanged;

            if (matchTimer != null)
            {
                matchTimer.timeChanged += OnTimeChanged;
                matchTimer.matchEnd += OnMatchEnd;
            }

            if (scoreHandler == null)
            {
                Debug.LogError("OnStartAuthority, scoreHandler not found");
                return;
            }
                
            if (matchTimer == null)
            {
                Debug.LogError("OnStartAuthority, matchTimer not found");
                return;
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

            if (scoreHandler == null)
            {
                Debug.LogError("OnDestroy, scoreHandler not found");
                return;
            }
            
            if (matchTimer == null)
            {
                Debug.LogError("OnDestroy, matchTimer not found");
                return;
            }

            StopCoroutine(updateManaBarCoroutine);
        }

        private void OnMatchEnd()
        {
            if (matchEndUI == null)
            {
                Debug.LogError("OnMatchEnd, matchEndUI not found");
                return;
            }
               
            ScoreHandler scoreHandler = FindObjectOfType<ScoreHandler>();

            if (scoreHandler == null)
            {
                Debug.LogError("There is no ScoreHandler in the current scene. Did you forget to add one?");
                return;
            }

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

        private IEnumerator UpdateManaBar()
        {
            while (true)
            {
                Rect uvRect = barRawImage.uvRect;
                uvRect.x -= 0.1f * Time.deltaTime;
                barRawImage.uvRect = uvRect;

                Vector2 barMaskSize = barMask.rectTransform.sizeDelta;
                barMaskSize.x = spellcasting.GetManaNormalized() * barMaskWidth;
                barMask.rectTransform.sizeDelta = barMaskSize;
                yield return null;
            }
        }

        private void OnTimeChanged(int minutes, int seconds)
        {
            if (seconds >= 10)
                timeText.text = minutes + ":" + seconds;
            else
                timeText.text = minutes + ":0" + seconds;
        }

        private void OnScoreChanged(Team team, int newScore)
        {
            goalScoredUI.SetActive(true);
            TMP_Text goalScoredText = goalScoredUI.GetComponentInChildren<TMP_Text>();
            if (goalScoredText != null)
                goalScoredText.text = $"<color=\"{team.ToString().ToLower()}\">{team.ToString().ToUpper()}</color> TEAM SCORES";
            StartCoroutine(DisableGoalScoredUI());

            if (goalScoredText == null)
            {
                Debug.LogError("OnScoreChanged, goalScoreText not found");
                return;
            }

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