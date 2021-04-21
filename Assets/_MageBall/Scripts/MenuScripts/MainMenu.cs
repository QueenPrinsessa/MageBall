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
                titleScreenPanel.SetActive(false);
            }

            menuButtonController = titleScreenPanel.GetComponent<MenuButtonController>();
            NetworkManagerMageBall.stopClient += OnStopClient;
        }

        private void OnEnable()
        {
            NetworkManagerMageBall.clientConnected += OnClientConnected;
            NetworkManagerMageBall.clientDisconnected += OnClientDisconnected;
        }

        private void OnDisable()
        {
            NetworkManagerMageBall.clientConnected -= OnClientConnected;
            NetworkManagerMageBall.clientDisconnected -= OnClientDisconnected;

        }

        private void OnDestroy()
        {
            NetworkManagerMageBall.stopClient -= OnStopClient;
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
            string ipAddress = ipAddressInputField.text;
            ipAddress = ipAddress.Trim();
            networkManager.networkAddress = ipAddress;
            networkManager.StartClient();

            menuButtonController.DeactivateButtons();

        }

        public void OpenOptionsMenu()
        {
            Debug.Log("Options pressed");
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