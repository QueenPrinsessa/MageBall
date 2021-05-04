using Mirror;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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
        [SerializeField] private RawImage manaBarRawImage;
        [SerializeField] private Mask manaBarMask;
        [SerializeField] private Spellcasting spellcasting;
        [SerializeField] private GameObject pauseMenuPrefab;
        [SerializeField] private float barMovingSpeed = 0.1f;

        private float manaBarMaskWidth;
        private Coroutine updateManaBarCoroutine;
        private PauseMenu pauseMenu;
        private NetworkGamePlayerMageBall networkGamePlayerMageBall;
        private bool isCountingDownUntilUnfreeze = false;

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
            manaBarMaskWidth = manaBarRawImage.rectTransform.rect.width;

            if (scoreHandler != null)
                scoreHandler.ScoreChanged += OnScoreChanged;
            else
                Debug.LogError("OnStartAuthority, scoreHandler not found");

            if (matchTimer != null)
            {
                matchTimer.TimeChanged += OnTimeChanged;
                matchTimer.MatchEnd += OnMatchEnd;
            }
            else
                Debug.LogError("OnStartAuthority, matchTimer not found");

            foreach (NetworkGamePlayerMageBall gamePlayer in NetworkManager.NetworkGamePlayers)
            {
                if (gamePlayer.connectionToClient == connectionToClient)
                {
                    networkGamePlayerMageBall = gamePlayer;
                    break;
                }
            }

            if (networkGamePlayerMageBall == null)
            {
                Debug.Log("NetworkGamePlayerMageBall couldn't be found in HUD script!");
            }

            GameObject pauseMenuUI = Instantiate(pauseMenuPrefab);
            pauseMenu = pauseMenuUI.GetComponent<PauseMenu>();
            pauseMenu.NetworkGamePlayer = networkGamePlayerMageBall;

            pauseMenu.PauseMenuOpened += OnPauseMenuOpened;
            pauseMenu.PauseMenuClosed += OnPauseMenuClosed;
        }

        [ClientCallback]
        private void Update()
        {
            if (!hasAuthority || networkGamePlayerMageBall == null)
                return;

            if (networkGamePlayerMageBall.IsFrozen && !isCountingDownUntilUnfreeze)
                StartCoroutine(CountdownUntilUnfreeze());
        }

        private IEnumerator CountdownUntilUnfreeze()
        {
            float countdownLength = NetworkManager.WaitBeforeControlsEnableInSeconds;
            isCountingDownUntilUnfreeze = true;
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
            isCountingDownUntilUnfreeze = false;
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
            else
                Debug.LogError("OnDestroy, scoreHandler not found");

            if (matchTimer != null)
            {
                matchTimer.TimeChanged -= OnTimeChanged;
                matchTimer.MatchEnd -= OnMatchEnd;
            }
            else
                Debug.LogError("OnDestroy, matchTimer not found");

            if (updateManaBarCoroutine != null)
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
                Rect uvRect = manaBarRawImage.uvRect;
                uvRect.x -= barMovingSpeed * Time.deltaTime;
                manaBarRawImage.uvRect = uvRect;

                Vector2 manaBarMaskSizeDelta = manaBarMask.rectTransform.sizeDelta;
                manaBarMaskSizeDelta.x = spellcasting.ManaNormalized * manaBarMaskWidth;
                manaBarMask.rectTransform.sizeDelta = manaBarMaskSizeDelta;
                
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
            {
                goalScoredText.text = $"<color=\"{team.ToString().ToLower()}\">{team.ToString().ToUpper()}</color> TEAM SCORES";
            }
            else
            {
                Debug.LogError("OnScoreChanged, goalScoreText not found");
                return;
            }
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