using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MageBall
{
    public class PauseMenu : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private GameObject pauseCanvas;
        [SerializeField] private MenuButtonController menuButtonController;
        private OptionsMenu optionsMenu;

        public event Action PauseMenuOpened;
        public event Action PauseMenuClosed;

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

        public bool IsOpen => pauseCanvas.activeInHierarchy;

        private void Start()
        {
            optionsMenu = GetComponentInChildren<OptionsMenu>();

            if (optionsMenu == null)
            {
                Debug.LogError("There is no Options Menu in the Pause Menu.");
                return;
            }

            optionsMenu.OptionsMenuOpened += OnOptionsMenuOpened;
            optionsMenu.OptionsMenuClosed += OnOptionsMenuClosed;
        }

        private void Update()
        {
            if (pauseCanvas.activeInHierarchy)
            {
                if (Input.GetButtonDown("Cancel"))
                    CloseMenu();
            }
            else
            {
                if (Input.GetButtonDown("Cancel"))
                    OpenMenu();
            }
        }


        private void OnOptionsMenuOpened()
        {
            menuButtonController.DeactivateButtons();
        }

        private void OnOptionsMenuClosed()
        {
            menuButtonController.ActivateButtons();
        }

        public void OpenMenu()
        {
            pauseCanvas.SetActive(true);
            PauseMenuOpened?.Invoke();
        }

        public void CloseMenu()
        {
            pauseCanvas.SetActive(false);
            PauseMenuClosed?.Invoke();
        }

        public void OpenOptions()
        {
            if (optionsMenu == null)
                return;
            optionsMenu.OpenMenu();
        }

        public void LeaveGame()
        {
            NetworkManager.StopHost();
        }

    }
}