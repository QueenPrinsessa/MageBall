using Mirror;
using System.Collections;
using TMPro;
using UnityEngine;

namespace MageBall
{
    /// <summary>
    /// Huvudansvarig: Zoe
    /// </summary>
    public class MainMenu : MonoBehaviour
    {
        [Header("Networking")]
        [SerializeField] private GameObject networkManagerPrefab;
        private NetworkManagerMageBall networkManager;

        [Header("UI")]
        [SerializeField] private TMP_InputField ipAddressInputField;
        [SerializeField] private GameObject titleScreenPanel;
        [SerializeField] private OptionsMenu optionsMenu;
        [SerializeField] private GameObject attemptingConnectionPanel;
        [SerializeField] private PopUp howToPlayPopUp;
        [SerializeField] private PopUp controlsPopUp;
        [SerializeField] private PopUp creditsPopUp;


        private MenuButtonController menuButtonController;

        private void Start()
        {
            Cursor.lockState = CursorLockMode.None;

            if (networkManager == null && NetworkManager.singleton == null)
            {
                GameObject networkManagerGameObject = Instantiate(networkManagerPrefab);
                networkManager = networkManagerGameObject.GetComponent<NetworkManagerMageBall>();
                if (networkManager == null)
                    Debug.LogError("There is no NetworkManagerMageBall component on the networkmanager prefab. Did you accidentally use the default NetworkManager instead?");
            }
            else if (networkManager == null)
            {
                networkManager = (NetworkManagerMageBall)NetworkManager.singleton;
                if(networkManager.isNetworkActive)
                    titleScreenPanel.SetActive(false);
            }

            optionsMenu = GetComponentInChildren<OptionsMenu>();
            if (optionsMenu == null)
                Debug.LogError("There is no Options Menu in the Main Menu!");

            menuButtonController = titleScreenPanel.GetComponent<MenuButtonController>();
            NetworkManagerMageBall.ClientStopped += OnStopClient;
        }

        private void OnEnable()
        {
            NetworkManagerMageBall.ClientConnected += OnClientConnected;
            NetworkManagerMageBall.ClientDisconnected += OnClientDisconnected;
            optionsMenu.OptionsMenuOpened += OnOptionsMenuOpened;
            optionsMenu.OptionsMenuClosed += OnOptionsMenuClosed;
            howToPlayPopUp.PopUpOpened += OnPopUpOpened;
            howToPlayPopUp.PopUpClosed += OnPopUpClosed;
            controlsPopUp.PopUpOpened += OnPopUpOpened;
            controlsPopUp.PopUpClosed += OnPopUpClosed;
            creditsPopUp.PopUpOpened += OnPopUpOpened;
            creditsPopUp.PopUpClosed += OnPopUpClosed;
        }

        private void OnDisable()
        {
            NetworkManagerMageBall.ClientConnected -= OnClientConnected;
            NetworkManagerMageBall.ClientDisconnected -= OnClientDisconnected;
            optionsMenu.OptionsMenuOpened -= OnOptionsMenuOpened;
            optionsMenu.OptionsMenuClosed -= OnOptionsMenuClosed;
            howToPlayPopUp.PopUpOpened -= OnPopUpOpened;
            howToPlayPopUp.PopUpClosed -= OnPopUpClosed;
            controlsPopUp.PopUpOpened -= OnPopUpOpened;
            controlsPopUp.PopUpClosed -= OnPopUpClosed;
            creditsPopUp.PopUpOpened -= OnPopUpOpened;
            creditsPopUp.PopUpClosed -= OnPopUpClosed;
        }

        private void OnPopUpOpened()
        {
            menuButtonController.DeactivateButtons();
        }


        private void OnPopUpClosed()
        {
            menuButtonController.ActivateButtons();
        }

        private void OnDestroy()
        {
            NetworkManagerMageBall.ClientStopped -= OnStopClient;
        }
        private void OnOptionsMenuOpened()
        {
            menuButtonController.DeactivateButtons();
        }

        private void OnOptionsMenuClosed()
        {
            menuButtonController.ActivateButtons();
        }

        private void OnStopClient()
        {
            attemptingConnectionPanel.SetActive(false);
            titleScreenPanel.SetActive(true);
        }

        private void OnClientConnected()
        {
            menuButtonController.ActivateButtons();
            titleScreenPanel.SetActive(false);
            attemptingConnectionPanel.SetActive(false);
        }

        private void OnClientDisconnected()
        {
            attemptingConnectionPanel.SetActive(false);
            menuButtonController.ActivateButtons();
        }

        public void HideIpAddressField()
        {
            ipAddressInputField.gameObject.SetActive(false);
        }

        public void ShowIpAddressField()
        {
            ipAddressInputField.gameObject.SetActive(true);
        }

        public void CreateGame()
        {
            if (networkManager == null)
            {
                Debug.LogError("There is no NetworkManager assigned to the main menu!");
                return;
            }

            networkManager.StartHost();
            titleScreenPanel.SetActive(false);
        }

        public void JoinGame()
        {
            string ipAddress = ipAddressInputField.text.Trim();

            if (string.IsNullOrEmpty(ipAddress))
                return;

            networkManager.networkAddress = ipAddress;
            networkManager.StartClient();

            menuButtonController.DeactivateButtons();
            attemptingConnectionPanel.SetActive(true);
            AudioSource waitMusicSource = attemptingConnectionPanel.GetComponent<AudioSource>();
            StartCoroutine(PlayAudioWithDelay(waitMusicSource, 0.1f));
        }

        private IEnumerator PlayAudioWithDelay(AudioSource source, float delayInSeconds)
        {
            source.Stop();
            yield return new WaitForSeconds(delayInSeconds);
            if(source.isActiveAndEnabled)
                source.Play();
        }

        public void OpenOptionsMenu()
        {
            optionsMenu.OpenMenu();
        }

        public void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif

#if UNITY_STANDALONE
            Application.Quit();
#endif
        }



    }
}