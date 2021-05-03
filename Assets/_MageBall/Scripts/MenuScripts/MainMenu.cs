using Mirror;
using TMPro;
using UnityEngine;

namespace MageBall
{
    public class MainMenu : MonoBehaviour
    {
        [Header("Networking")]
        [SerializeField] private GameObject networkManagerPrefab;
        private NetworkManagerMageBall networkManager;

        [Header("UI")]
        [SerializeField] private TMP_InputField ipAddressInputField;
        [SerializeField] private GameObject titleScreenPanel;
        [SerializeField] private OptionsMenu optionsMenu;
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
        }

        private void OnDisable()
        {
            NetworkManagerMageBall.ClientConnected -= OnClientConnected;
            NetworkManagerMageBall.ClientDisconnected -= OnClientDisconnected;
            optionsMenu.OptionsMenuOpened -= OnOptionsMenuOpened;
            optionsMenu.OptionsMenuClosed -= OnOptionsMenuClosed;

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
            titleScreenPanel.SetActive(true);
        }

        private void OnClientConnected()
        {
            menuButtonController.ActivateButtons();
            titleScreenPanel.SetActive(false);
        }

        private void OnClientDisconnected()
        {
            menuButtonController.ActivateButtons();
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
            ipAddress = string.IsNullOrEmpty(ipAddress) ? "localhost" : ipAddress;
            networkManager.networkAddress = ipAddress;
            networkManager.StartClient();

            menuButtonController.DeactivateButtons();

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