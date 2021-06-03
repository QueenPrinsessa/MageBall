using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MageBall
{
    /// <summary>
    /// Huvudansvarig: Zoe
    /// </summary>
    public class PauseMenu : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private GameObject pauseCanvas;
        [SerializeField] private MenuButtonController menuButtonController;
        [SerializeField] private PopUp howToPlayPopUp;
        [SerializeField] private PopUp controlsPopUp;

        private OptionsMenu optionsMenu;

        public event Action PauseMenuOpened;
        public event Action PauseMenuClosed;
        public NetworkGamePlayerMageBall NetworkGamePlayer { get; set; }

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
            howToPlayPopUp.PopUpOpened += OnPopUpOpened;
            howToPlayPopUp.PopUpClosed += OnPopUpClosed;
            controlsPopUp.PopUpOpened += OnPopUpOpened;
            controlsPopUp.PopUpClosed += OnPopUpClosed;
        }

        private void OnDestroy()
        {
            optionsMenu.OptionsMenuOpened -= OnOptionsMenuOpened;
            optionsMenu.OptionsMenuClosed -= OnOptionsMenuClosed;
            howToPlayPopUp.PopUpOpened -= OnPopUpOpened;
            howToPlayPopUp.PopUpClosed -= OnPopUpClosed;
            controlsPopUp.PopUpOpened -= OnPopUpOpened;
            controlsPopUp.PopUpClosed -= OnPopUpClosed;
        }

        private void Update()
        {
            if (IsOpen && !optionsMenu.IsOpen)
            {
                if (Input.GetButtonDown("Cancel"))
                    CloseMenu();
            }
            else
            {
                if (Input.GetButtonDown("Pause"))
                    OpenMenu();
            }
        }

        private void OnPopUpOpened()
        {
            menuButtonController.DeactivateButtons();
        }


        private void OnPopUpClosed()
        {
            menuButtonController.ActivateButtons();
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
            if (NetworkGamePlayer == null)
            {
                Debug.LogError("NetworkGamePlayer instance hasn't been set in Pause Menu script. Make sure it is set when spawning menu!!");
                return;
            }

            NetworkGamePlayer.Disconnect();
        }

    }
}