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
        [SerializeField] private TMP_Text countdownText;
        [SerializeField] private GameObject goalScoredUI;
        [SerializeField] private GameObject matchEndUI;
        [SerializeField] private RawImage barRawImage;
        [SerializeField] private Mask barMask;
        [SerializeField] private Spellcasting spellcasting;
        [SerializeField] private GameObject pauseMenuPrefab;
        
        private float barMaskWidth;
        private Coroutine updateManaBarCoroutine;
        private PauseMenu pauseMenu;
        [SyncVar] private NetworkGamePlayerMageBall networkGamePlayerMageBall;
        private bool isCountingDown = false;

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

            if (scoreHandler != null)
                scoreHandler.ScoreChanged += OnScoreChanged;

            if (matchTimer != null)
            {
                matchTimer.TimeChanged += OnTimeChanged;
                matchTimer.MatchEnd += OnMatchEnd;
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

            GameObject pauseMenuUI = Instantiate(pauseMenuPrefab);
            pauseMenu = pauseMenuUI.GetComponent<PauseMenu>();

            if(pauseMenu != null)
                pauseMenu.NetworkGamePlayer = networkGamePlayerMageBall;

            pauseMenu.PauseMenuOpened += OnPauseMenuOpened;
            pauseMenu.PauseMenuClosed += OnPauseMenuClosed;
        }

        [ClientCallback]
        private void Update()
        {
            if (!hasAuthority || networkGamePlayerMageBall == null)
                return;

            if (networkGamePlayerMageBall.IsFrozen && !isCountingDown)
                StartCoroutine(CountdownUntilUnfreeze());
        }

        [Server]
        public void SetNetworkGamePlayer(NetworkGamePlayerMageBall networkGamePlayer)
        {
            networkGamePlayerMageBall = networkGamePlayer;
        }
        

        private IEnumerator CountdownUntilUnfreeze()
        {
            float countdownLength = NetworkManager.WaitBeforeControlsEnableInSeconds;
            isCountingDown = true;
            int seconds = Mathf.RoundToInt(countdownLength);

            countdownText.gameObject.SetActive(true);
            while (networkGamePlayerMageBall.IsFrozen)
            {
                countdownText.text = seconds.ToString();
                yield return new WaitForSeconds(1);
                seconds--;
            }
            countdownText.text = "GO";

            yield return new WaitForSeconds(1);

            countdownText.gameObject.SetActive(false);
            isCountingDown = false;
        }

        private void OnPauseMenuOpened()
        {
            Cursor.lockState = CursorLockMode.None;

            PlayerMovement playerMovement = GetComponent<PlayerMovement>();
            ThirdPersonCamera thirdPersonCamera = GetComponent<ThirdPersonCamera>();
            Spellcasting spellcasting = GetComponent<Spellcasting>();

            if (playerMovement != null) 
                playerMovement.enabled = false;

            if (thirdPersonCamera != null) 
                thirdPersonCamera.enabled = false;

            if (spellcasting != null)
                spellcasting.CmdSetCanCastSpells(false);
        }

        private void OnPauseMenuClosed()
        {
            Cursor.lockState = CursorLockMode.Locked;

            PlayerMovement playerMovement = GetComponent<PlayerMovement>();
            ThirdPersonCamera thirdPersonCamera = GetComponent<ThirdPersonCamera>();
            Spellcasting spellcasting = GetComponent<Spellcasting>();

            if (playerMovement != null && !networkGamePlayerMageBall.IsFrozen)
                playerMovement.enabled = true;

            if (thirdPersonCamera != null)
                thirdPersonCamera.enabled = true;

            if (spellcasting != null && !networkGamePlayerMageBall.IsFrozen)
                spellcasting.CmdSetCanCastSpells(true);

        }

        [ClientCallback]
        private void OnDestroy()
        {
            if (hasAuthority)
            {
                pauseMenu.PauseMenuOpened -= OnPauseMenuOpened;
                pauseMenu.PauseMenuClosed -= OnPauseMenuClosed;
            }

            ScoreHandler scoreHandler = FindObjectOfType<ScoreHandler>();
            MatchTimer matchTimer = FindObjectOfType<MatchTimer>();

            if (scoreHandler != null)
                scoreHandler.ScoreChanged -= OnScoreChanged;

            if (matchTimer != null)
            {
                matchTimer.TimeChanged -= OnTimeChanged;
                matchTimer.MatchEnd -= OnMatchEnd;
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

            if(updateManaBarCoroutine != null)
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