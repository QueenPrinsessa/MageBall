using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using TMPro;

namespace MageBall
{
    public class PlayerNameTag : NetworkBehaviour
    {
        [SyncVar(hook = nameof(OnNameChange))]
        private string playerName;
        [SerializeField] private TMP_Text nameTag;

        void OnNameChange(string oldPlayerName, string newPlayerName)
        {
            nameTag.text = newPlayerName;
            //RpcSetPlayerName(playerName);
        }

        //[ClientRpc]
        //void RpcSetPlayerName(string name)
        //{
        //    nameTag.text = name;
        //}

        [Server]
        public void SetPlayerName(string name)
        {
            playerName = name;
        }
    }
}