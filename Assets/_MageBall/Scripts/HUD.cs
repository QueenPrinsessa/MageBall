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
        [SerializeField] private Image manaBarBorder;
        [SerializeField] private Color powerUpColor;
        [SerializeField] private float barMovingSpeed = 0.1f;

        [Header("Sounds")]
        [SerializeField] private AudioClip[] countdownBeepClips;
        [SerializeField] private AudioClip countdownGoClip;
        [SerializeField] private AudioSource audioSource;

        private float manaBarMaskWidth;
        private Coroutine updateManaBarCoroutine;
        private PauseMenu pauseMenu;
        [SyncVar(hook = nameof(OnNetworkGamePlayerMageBallChanged))] private NetworkGamePlayerMageBall networkGamePlayerMageBall;
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

        [TargetRpc]
        public void TargetPowerUpManaBar(float duration)
        {
            StartCoroutine(PowerUpManaBar(duration));
        }

        public IEnumerator PowerUpManaBar(float duration)
        {
            Color borderColor = manaBarBorder.color;
            manaBarBorder.color = powerUpColor;
            yield return new WaitForSeconds(duration);
            manaBarBorder.color = borderColor;
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
                OnTimeChanged(matchTimer.Minutes, matchTimer.Seconds);
                matchTimer.TimeChanged += OnTimeChanged;
                matchTimer.MatchEnd += OnMatchEnd;
            }
            else
                Debug.LogError("OnStartAuthority, matchTimer not found");

            GameObject pauseMenuUI = Instantiate(pauseMenuPrefab);
            pauseMenu = pauseMenuUI.GetComponent<PauseMenu>();

            pauseMenu.PauseMenuOpened += OnPauseMenuOpened;
            pauseMenu.PauseMenuClosed += OnPauseMenuClosed;
        }

        [Server]
        public void SetNetworkGamePlayerMageBall(NetworkGamePlayerMageBall networkGamePlayerMageBall)
        {
            this.networkGamePlayerMageBall = networkGamePlayerMageBall;
        }

        private void OnNetworkGamePlayerMageBallChanged(NetworkGamePlayerMageBall oldValue, NetworkGamePlayerMageBall newValue)
        {
            //Pause menu is spawned after NetworkGamePlayer is initially set on Server so we need to wait to set it
            StartCoroutine(WaitToSetPauseMenuNetworkGamePlayer(newValue));
        }

        private IEnumerator WaitToSetPauseMenuNetworkGamePlayer(NetworkGamePlayerMageBall networkGamePlayerMageBall)
        {
            yield return new WaitUntil(() => pauseMenu != null);

            pauseMenu.NetworkGamePlayer = networkGamePlayerMageBall;
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
                audioSource.PlayOneShot(countdownBeepClips[Random.Range(0, countdownBeepClips.Length)]);
                yield return new WaitForSeconds(1);
                seconds--;
            }
            countdownText.text = "GO";
            audioSource.PlayOneShot(countdownGoClip);

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
            Animator animator = GetComponent<Animator>();
            Dancing dancing = GetComponent<Dancing>();

            if (animator != null)
            {
                animator.SetFloat("Speed", 0);
                animator.SetBool("IsJumping", false);
            }
            if (playerMovement != null)
            {
                playerMovement.ResetSpeed();
                playerMovement.enabled = false;
            }

            if (dancing != null)
            {
                dancing.enabled = false;
                dancing.StopDancing();
            }

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
            Dancing dancing = GetComponent<Dancing>();

            if (playerMovement != null && !networkGamePlayerMageBall.IsFrozen)
                playerMovement.enabled = true;

            if (dancing != null)
                dancing.enabled = true;

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

            Debug.Log("Text activated, Winner: " + scoreHandler.Winner );

            switch (scoreHandler.Winner)
            {
                case Winner.RedTeam:
                    matchEndText.text = $"<color=red>{Names.RedTeamName}</color> WIN!";
                    break;
                case Winner.BlueTeam:
                    matchEndText.text = $"<color=blue>{Names.BlueTeamName}</color> TEAM WINS!";
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
            if(goalScoredText == null)
            {
                Debug.LogError("OnScoreChanged, goalScoredText not found");
                return;
            }
            StartCoroutine(DisableGoalScoredUI());

            string color = "white";
            switch (team)
            {
                case Team.Red:
                    color = "red";
                    redTeamScoreText.text = newScore.ToString();
                    break;
                case Team.Blue:
                    color = "blue";
                    blueTeamScoreText.text = newScore.ToString();
                    break;
            }

            goalScoredText.text = $"<color={color}>{Names.NameFromTeam(team).ToUpper()}</color> SCORE";
        }

        private IEnumerator DisableGoalScoredUI()
        {
            yield return new WaitForSeconds(NetworkManager.WaitBeforeResetAfterGoalInSeconds);
            goalScoredUI.SetActive(false);
        }
    }
}