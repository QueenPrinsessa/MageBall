using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MageBall
{
    public class MainMenu : MonoBehaviour
    {
        [Header("Networking")]
        [SerializeField] private NetworkManagerMageBall networkManager;

        [Header("UI")]
        [SerializeField] private TMP_InputField ipAddressInputField;
        [SerializeField] private GameObject titleScreenPanel;
        private MenuButtonController menuButtonController;

        private void Start()
        {
            menuButtonController = titleScreenPanel.GetComponent<MenuButtonController>();
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