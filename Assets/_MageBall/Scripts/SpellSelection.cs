using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MageBall
{
    public class SpellSelection : NetworkBehaviour
    {
        public static readonly string MainSpellPlayerPrefsKey = "MainSpell";
        public static readonly string OffhandSpellPlayerPrefsKey = "OffhandSpell";
        public static readonly string ExtraSpellPlayerPrefsKey = "ExtraSpell";
        public static readonly string PlayerModelPlayerPrefsKey = "PlayerModel";
        public static readonly string PassivePlayerPrefsKey = "Passive";

        [Header("Network")]
        [SerializeField] private NetworkRoomPlayerMageBall roomPlayer;

        [Header("UI")]
        [SerializeField] private TMP_Dropdown mainSpellDropdown;
        [SerializeField] private TMP_Dropdown offhandSpellDropdown;
        [SerializeField] private TMP_Dropdown extraSpellDropdown;
        [SerializeField] private TMP_Dropdown passiveDropdown;
        [SerializeField] private Button malePlayerModelButton;
        [SerializeField] private Button femalePlayerModelButton;
        [SerializeField] private PopUp spellInfoPopUp;
        [SerializeField] private MenuButtonController menuButtonController;

        private int oldMainValue;
        private int oldOffhandValue;
        private int oldExtraValue;

        private Spells MainSpell => (Spells)mainSpellDropdown.value;
        private Spells OffhandSpell => (Spells)offhandSpellDropdown.value;
        private Spells ExtraSpell => (Spells)extraSpellDropdown.value;
        private PlayerModel PlayerModel { get; set; }
        private Passives Passive => (Passives)passiveDropdown.value;

        public override void OnStartAuthority()
        {
            SetupSpellDropdowns();
            SetupPassiveDropdown();
            SetupPlayerModelButtons();

            UpdatePlayerLoadout();

            spellInfoPopUp.PopUpOpened += OnPopUpOpened;
            spellInfoPopUp.PopUpClosed += OnPopUpClosed;
        }

        private void OnPopUpOpened()
        {
            menuButtonController.DeactivateButtons();
        }

        private void OnPopUpClosed()
        {
            menuButtonController.ActivateButtons();
        }


        private void SetupSpellDropdowns()
        {
            mainSpellDropdown.ClearOptions();
            offhandSpellDropdown.ClearOptions();
            extraSpellDropdown.ClearOptions();

            var spellOptions = new List<string>();
            foreach (string spellName in Enum.GetNames(typeof(Spells)))
            {
                string spellNameWithSpaces = Regex.Replace(spellName, "([a-z])_?([A-Z])", "$1 $2");
                spellOptions.Add(spellNameWithSpaces);
            }

            int mainSpellSelection = PlayerPrefs.GetInt(MainSpellPlayerPrefsKey, (int)Spells.ForcePush);
            int offhandSpellSelection = PlayerPrefs.GetInt(OffhandSpellPlayerPrefsKey, (int)Spells.ForcePull);
            int extraSpellSelection = PlayerPrefs.GetInt(ExtraSpellPlayerPrefsKey, (int)Spells.ForceFly);
            PlayerPrefs.SetInt(MainSpellPlayerPrefsKey, mainSpellSelection);
            PlayerPrefs.SetInt(OffhandSpellPlayerPrefsKey, offhandSpellSelection);
            PlayerPrefs.SetInt(ExtraSpellPlayerPrefsKey, extraSpellSelection);
            oldMainValue = mainSpellSelection;
            oldOffhandValue = offhandSpellSelection;
            oldExtraValue = extraSpellSelection;

            InitializeDropdown(mainSpellDropdown, spellOptions, mainSpellSelection);
            InitializeDropdown(offhandSpellDropdown, spellOptions, offhandSpellSelection);
            InitializeDropdown(extraSpellDropdown, spellOptions, extraSpellSelection);
            mainSpellDropdown.onValueChanged.AddListener(OnMainSpellDropdownChanged);
            offhandSpellDropdown.onValueChanged.AddListener(OnOffhandSpellDropdownChanged);
            extraSpellDropdown.onValueChanged.AddListener(OnExtraSpellDropdownChanged);
        }

        private void SetupPassiveDropdown()
        {
            passiveDropdown.ClearOptions();
            var passiveOptions = new List<string>();
            foreach (string passiveName in Enum.GetNames(typeof(Passives)))
            {
                string passiveNameWithSpaces = Regex.Replace(passiveName, "([a-z])_?([A-Z])", "$1 $2");
                passiveOptions.Add(passiveNameWithSpaces);
            }

            int passiveSelection = PlayerPrefs.GetInt(PassivePlayerPrefsKey, (int) Passives.FasterSpeed);
            PlayerPrefs.SetInt(PassivePlayerPrefsKey, passiveSelection);
            
            InitializeDropdown(passiveDropdown, passiveOptions, passiveSelection);
            passiveDropdown.onValueChanged.AddListener(OnPassiveDropdownChanged);
        }

        private void SetupPlayerModelButtons()
        {
            PlayerModel = (PlayerModel)PlayerPrefs.GetInt(PlayerModelPlayerPrefsKey, (int)PlayerModel.Man);
            PlayerPrefs.SetInt(PlayerModelPlayerPrefsKey, (int)PlayerModel);
            switch (PlayerModel)
            {
                case PlayerModel.Man:
                    malePlayerModelButton.interactable = false;
                    femalePlayerModelButton.interactable = true;
                    break;
                case PlayerModel.Woman:
                    femalePlayerModelButton.interactable = false;
                    malePlayerModelButton.interactable = true;
                    break;
            }

            malePlayerModelButton.onClick.AddListener(OnMalePlayerModelButtonClick);
            femalePlayerModelButton.onClick.AddListener(OnFemalePlayerModelButtonClick);
        }

        private void OnPassiveDropdownChanged(int newPassive)
        {
            PlayerPrefs.SetInt(PassivePlayerPrefsKey, newPassive);
            UpdatePlayerLoadout();
        }

        private void OnMalePlayerModelButtonClick()
        {
            malePlayerModelButton.interactable = false;
            femalePlayerModelButton.interactable = true;
            PlayerModel = PlayerModel.Man;
            PlayerPrefs.SetInt(PlayerModelPlayerPrefsKey, (int)PlayerModel);
            UpdatePlayerLoadout();
        }

        private void OnFemalePlayerModelButtonClick()
        {
            femalePlayerModelButton.interactable = false;
            malePlayerModelButton.interactable = true;
            PlayerModel = PlayerModel.Woman;
            PlayerPrefs.SetInt(PlayerModelPlayerPrefsKey, (int)PlayerModel);
            UpdatePlayerLoadout();
        }

        private void OnDestroy()
        {
            mainSpellDropdown.onValueChanged.RemoveListener(OnMainSpellDropdownChanged);
            offhandSpellDropdown.onValueChanged.RemoveListener(OnOffhandSpellDropdownChanged);
            extraSpellDropdown.onValueChanged.RemoveListener(OnExtraSpellDropdownChanged);
            malePlayerModelButton.onClick.RemoveListener(OnMalePlayerModelButtonClick);
            femalePlayerModelButton.onClick.RemoveListener(OnFemalePlayerModelButtonClick);
            passiveDropdown.onValueChanged.RemoveListener(OnPassiveDropdownChanged);
        }

        private void InitializeDropdown(TMP_Dropdown dropDown, List<string> options, int selection)
        {
            dropDown.AddOptions(options);
            dropDown.value = selection;
            dropDown.RefreshShownValue();
        }

        private void OnMainSpellDropdownChanged(int mainValue)
        {
            int offhandValue = offhandSpellDropdown.value;
            int extraValue = extraSpellDropdown.value;

            if (offhandValue == mainValue)
                offhandSpellDropdown.value = oldMainValue;
            else if (extraValue == mainValue)
                extraSpellDropdown.value = oldMainValue;

            PlayerPrefs.SetInt(MainSpellPlayerPrefsKey, mainValue);
            oldMainValue = mainValue;
            UpdatePlayerLoadout();
        }

        private void OnOffhandSpellDropdownChanged(int offHandValue)
        {
            int mainValue = mainSpellDropdown.value;
            int extraValue = extraSpellDropdown.value;

            if (mainValue == offHandValue)
                mainSpellDropdown.value = oldOffhandValue;
            else if (extraValue == offHandValue)
                extraSpellDropdown.value = oldOffhandValue;

            PlayerPrefs.SetInt(OffhandSpellPlayerPrefsKey, offHandValue);
            oldOffhandValue = offHandValue;
            UpdatePlayerLoadout();
        }

        private void OnExtraSpellDropdownChanged(int extraValue)
        {
            int offhandValue = offhandSpellDropdown.value;
            int mainValue = mainSpellDropdown.value;

            if (offhandValue == extraValue)
                offhandSpellDropdown.value = oldExtraValue;
            else if (mainValue == extraValue)
                mainSpellDropdown.value = oldExtraValue;

            PlayerPrefs.SetInt(ExtraSpellPlayerPrefsKey, extraValue);
            oldExtraValue = extraValue;
            UpdatePlayerLoadout();
        }

        private void UpdatePlayerLoadout()
        {
            roomPlayer.CmdSetPlayerLoadout(MainSpell, OffhandSpell, ExtraSpell, PlayerModel, Passive);
        }

    }
}