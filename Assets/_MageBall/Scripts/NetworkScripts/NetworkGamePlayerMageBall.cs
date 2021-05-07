using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MageBall
{
    public class NetworkGamePlayerMageBall : NetworkBehaviour
    {
        private static readonly string DefaultPlayerName = "Player";

        [SyncVar] private string displayName = "Loading...";
        [SyncVar] private PlayerLoadout playerLoadout;
        private bool isFrozen;
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
        public PlayerLoadout PlayerLoadout => playerLoadout;
        public bool IsHost { get; private set; }
        public bool IsFrozen => isFrozen;

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
        public void SetPlayerLoadout(PlayerLoadout playerLoadout)
        {
            this.playerLoadout = playerLoadout;
        }

        [Server]
        public void SetPlayerGameObject(GameObject gameObject, Vector3 spawnPosition, Quaternion spawnRotation)
        {
            playerGameObject = gameObject;
            this.spawnPosition = spawnPosition;
            this.spawnRotation = spawnRotation;
        }

        [TargetRpc]
        public void TargetResetPlayerOwner()
        {
            StartCoroutine(ResetPlayer());
        }

        [TargetRpc]
        public void TargetResetPlayer(NetworkConnection connection)
        {
            StartCoroutine(ResetPlayer());
        }

        private IEnumerator ResetPlayer()
        {
            if (playerGameObject == null)
                yield return new WaitUntil(() => playerGameObject != null);

            PlayerMovement playerMovement = playerGameObject.GetComponent<PlayerMovement>();
            playerMovement.enabled = false;
            Spellcasting spellcasting = playerGameObject.GetComponent<Spellcasting>();
            spellcasting.ResetMana();
            spellcasting.CmdSetCanCastSpells(false);
            Animator animator = playerGameObject.GetComponent<Animator>();
            if (animator != null)
            {
                animator.SetFloat("Speed", 0);
                animator.SetBool("IsJumping", false);
            }
            playerGameObject.transform.position = spawnPosition;
            playerGameObject.transform.rotation = spawnRotation;
            StartCoroutine(EnablePlayerControls());
        }

        private IEnumerator EnablePlayerControls()
        {
            isFrozen = true;
            yield return new WaitForSeconds(NetworkManager.WaitBeforeControlsEnableInSeconds);
            isFrozen = false;

            PauseMenu pauseMenu = FindObjectOfType<PauseMenu>();
            if (pauseMenu != null && pauseMenu.IsOpen)
                yield break;

            PlayerMovement playerMovement = playerGameObject.GetComponent<PlayerMovement>();
            playerMovement.enabled = true;
            Spellcasting spellcasting = playerGameObject.GetComponent<Spellcasting>();
            spellcasting.CmdSetCanCastSpells(true);
        }

        public void Disconnect()
        {
            if (IsHost)
            {
                for (int i = NetworkManager.NetworkGamePlayers.Count; i-- > 0;)
                {
                    if (NetworkManager.NetworkGamePlayers[i] != this)
                        TargetDisconnect(NetworkManager.NetworkGamePlayers[i].connectionToClient);
                }
                StartCoroutine(StopHost());
                return;
            }

            NetworkManager.StopClient();
        }

        [TargetRpc]
        public void TargetDisconnect(NetworkConnection connection)
        {
            NetworkManager.StopClient();
        }

        private IEnumerator StopHost()
        {
            yield return new WaitUntil(() => NetworkManager.NetworkGamePlayers.Count == 1);

            NetworkManager.StopHost();
        }
    }
}