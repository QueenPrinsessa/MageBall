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

        [SyncVar] private string displayName = "Loading...";
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

        [SyncVar] private Vector3 spawnPosition;
        [SyncVar] private Quaternion spawnRotation;
        [SyncVar] private GameObject playerGameObject;

        public string DisplayName => displayName ?? DefaultPlayerName;

        [Command]
        private void CmdSetDisplayName(string displayName)
        {
            this.displayName = displayName;
        }

        public override void OnStartClient()
        {
            DontDestroyOnLoad(gameObject); //might be unnecessary
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

        [Server]
        public void SetPlayerGameObject(GameObject gameObject, Vector3 spawnPosition, Quaternion spawnRotation)
        {
            playerGameObject = gameObject;
            this.spawnPosition = spawnPosition;
            this.spawnRotation = spawnRotation;
        }

        [TargetRpc]
        public void TargetResetPosition()
        {
            playerGameObject.GetComponent<PlayerMovement>().enabled = false;
            playerGameObject.transform.position = spawnPosition;
            playerGameObject.transform.rotation = spawnRotation;
            StartCoroutine(EnablePlayerControls());
        }

        [ClientCallback]
        private IEnumerator EnablePlayerControls()
        {
            if (playerGameObject == null)
                yield break;

            yield return new WaitForSeconds(1);
            playerGameObject.GetComponent<PlayerMovement>().enabled = true;
        }
    }
}