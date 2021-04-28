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
        [SyncVar] private Team team;
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

        public bool IsHost { get; private set; }
        public bool IsFrozen { get; private set; }

        [Command]
        private void CmdSetDisplayName(string displayName)
        {
            this.displayName = displayName;
        }

        public override void OnStartClient()
        {
            DontDestroyOnLoad(gameObject);
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
        public void SetIsHost(bool isHost)
        {
            IsHost = isHost;
        }

        [Server]
        public void SetTeam(Team team)
        {
            this.team = team;
        }

        [Server]
        public void SetPlayerGameObject(GameObject gameObject, Vector3 spawnPosition, Quaternion spawnRotation)
        {
            playerGameObject = gameObject;
            this.spawnPosition = spawnPosition;
            this.spawnRotation = spawnRotation;
        }

        [TargetRpc]
        public void TargetResetPlayer()
        {
            if (playerGameObject == null)
                return;

            playerGameObject.GetComponent<PlayerMovement>().enabled = false;
            playerGameObject.GetComponent<Spellcasting>().enabled = false;
            playerGameObject.transform.position = spawnPosition;
            playerGameObject.transform.rotation = spawnRotation;
            StartCoroutine(EnablePlayerControls());
        }

        [ClientCallback]
        private IEnumerator EnablePlayerControls()
        {
            if (playerGameObject == null)
                yield break;

            IsFrozen = true;

            yield return new WaitForSeconds(NetworkManager.WaitBeforeControlsEnableInSeconds);

            IsFrozen = false;

            PauseMenu pauseMenu = FindObjectOfType<PauseMenu>();
            if (pauseMenu != null && pauseMenu.IsOpen)
                yield break;

            playerGameObject.GetComponent<PlayerMovement>().enabled = true;
            playerGameObject.GetComponent<Spellcasting>().enabled = true;
        }
    }
}