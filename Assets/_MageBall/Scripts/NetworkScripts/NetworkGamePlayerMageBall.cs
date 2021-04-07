using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace MageBall
{
    public class NetworkGamePlayerMageBall : NetworkBehaviour
    {
        private static readonly string DefaultPlayerName = "Player";

        [SyncVar]
        private string displayName = "Loading...";

        public string DisplayName => displayName;

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

        [Command]
        private void CmdSetDisplayName(string displayName)
        {
            this.displayName = displayName;
        }

        public override void OnStartClient()
        {
            DontDestroyOnLoad(gameObject); //might be removeable
            NetworkManager.NetworkGamePlayers.Add(this);
        }

        public override void OnStopClient()
        {
            NetworkManager.NetworkGamePlayers.Remove(this);
        }

        [Server]
        public void SetDisplayName(string displayName)
        {
            this.displayName = displayName;
        }
    }
}