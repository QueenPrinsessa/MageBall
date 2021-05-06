using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;

namespace MageBall
{
    public class SpellSelection : NetworkBehaviour
    {
        public static readonly string MainSpellPlayerPrefsKey = "MainSpell";
        public static readonly string OffhandSpellPlayerPrefsKey = "OffhandSpell";
        public static readonly string ExtraSpellPlayerPrefsKey = "ExtraSpell";

        [Header("Network")]
        [SerializeField] private NetworkRoomPlayerMageBall roomPlayer;

        [Header("UI")]
        [SerializeField] private TMP_Dropdown mainSpellDropdown;
        [SerializeField] private TMP_Dropdown offhandSpellDropdown;
        [SerializeField] private TMP_Dropdown extraSpellDropdown;

        private int oldMainValue;
        private int oldOffhandValue;
        private int oldExtraValue;

        private Spells MainSpell => (Spells)mainSpellDropdown.value;
        private Spells OffhandSpell => (Spells)offhandSpellDropdown.value;
        private Spells ExtraSpell => (Spells)extraSpellDropdown.value;
        private PlayerModel PlayerModel => PlayerModel.Man;
        private Team Team => Team.Red;

        public override void OnStartAuthority()
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

            InitializeSpellDropdown(mainSpellDropdown, spellOptions, mainSpellSelection);
            InitializeSpellDropdown(offhandSpellDropdown, spellOptions, offhandSpellSelection);
            InitializeSpellDropdown(extraSpellDropdown, spellOptions, extraSpellSelection);
            mainSpellDropdown.onValueChanged.AddListener(OnMainSpellDropdownChanged);
            offhandSpellDropdown.onValueChanged.AddListener(OnOffhandSpellDropdownChanged);
            extraSpellDropdown.onValueChanged.AddListener(OnExtraSpellDropdownChanged);

            UpdatePlayerLoadout();
        }

        private void OnDestroy()
        {
            mainSpellDropdown.onValueChanged.RemoveListener(OnMainSpellDropdownChanged);
            offhandSpellDropdown.onValueChanged.RemoveListener(OnOffhandSpellDropdownChanged);
            extraSpellDropdown.onValueChanged.RemoveListener(OnExtraSpellDropdownChanged);
        }

        private void InitializeSpellDropdown(TMP_Dropdown dropDown, List<string> spellOptions, int selection)
        {
            dropDown.AddOptions(spellOptions);
            dropDown.value = selection;
            dropDown.RefreshShownValue();
        }

        public void OnMainSpellDropdownChanged(int mainValue)
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

        public void OnOffhandSpellDropdownChanged(int offHandValue)
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

        public void OnExtraSpellDropdownChanged(int extraValue)
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
            roomPlayer.CmdSetPlayerLoadout(MainSpell, OffhandSpell, ExtraSpell, PlayerModel, Team);
        }

    }
}