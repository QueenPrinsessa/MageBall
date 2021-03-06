using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using TMPro;

namespace MageBall
{
    /// <summary>
    /// Huvudansvarig: Hammi
    /// </summary>
    public class PlayerNameTag : NetworkBehaviour
    {
        [SyncVar(hook = nameof(OnNameChange))]
        private string playerName;
        [SerializeField] private TMP_Text nameTag;

        void OnNameChange(string oldPlayerName, string newPlayerName)
        {
            nameTag.text = newPlayerName;
        }

        [Server]
        public void SetPlayerName(string name)
        {
            playerName = name;
        }
    }
}