using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace MageBall
{
    /// <summary>
    /// Huvudansvarig: Zoe
    /// </summary>
    public class NameInput : MonoBehaviour
    {
        public const string PlayerPrefsDisplayNameKey = "DisplayName";

        private TMP_InputField inputField;
        [SerializeField] private MenuButtonController menuButtonController;
        //[SerializeField, Tooltip("Buttons that aren't pressable unless a name has been set")] private MenuButton[] lockedButtons;

        public string DisplayName { get; private set; }

        private void Start()
        {
            inputField = gameObject.GetComponent<TMP_InputField>();

            if (inputField == null)
            {
                Debug.LogError("There is no TMP_InputField attached to this component!");
                return;
            }

            SetupInputField();
        }

        private void SetupInputField()
        {
            string savedDisplayName = PlayerPrefs.GetString(PlayerPrefsDisplayNameKey, "");
            inputField.text = savedDisplayName;

            if (CheckValidName(savedDisplayName))
                DisplayName = savedDisplayName;
        }

        private bool CheckValidName(string name)
        {
            bool validName = !string.IsNullOrEmpty(name);

            if (validName)
                menuButtonController.ShowAllButtons();
            else
                menuButtonController.HideAllButtons();

            return validName;
        }

        public void SavePlayerNameToPlayerPrefs()
        {
            if (inputField == null)
                return;

            if (!CheckValidName(inputField.text))
                return;

            DisplayName = inputField.text;
            PlayerPrefs.SetString(PlayerPrefsDisplayNameKey, DisplayName);
        }
    }
}