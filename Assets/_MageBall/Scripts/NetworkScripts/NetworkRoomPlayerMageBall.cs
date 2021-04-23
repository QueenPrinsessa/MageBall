using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace MageBall
{
    public class NetworkRoomPlayerMageBall : NetworkBehaviour
    {
        private static readonly string DefaultPlayerName = "Player";

        [Header("UI")]
        [SerializeField] private GameObject lobbyUI;
        [SerializeField] private TMP_Text[] playerNameTexts = new TMP_Text[4];
        [SerializeField] private TMP_Text[] playerReadyTexts = new TMP_Text[4];
        [SerializeField] private MenuButton startGameButton;

        [SyncVar(hook = nameof(OnDisplayNameChanged))]
        [HideInInspector] private string displayName = "Loading...";
        [SyncVar(hook = nameof(OnReadyStatusChanged))]
        [HideInInspector] private bool isReady;

        public string DisplayName => displayName;
        public bool IsReady => isReady;

        private bool isHost;

        public bool IsHost
        {
            set
            {
                isHost = value;
                startGameButton.gameObject.SetActive(isHost);
            }
            get => isHost;
        }

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

        public override void OnStartAuthority()
        {
            string displayName = PlayerPrefs.GetString(NameInput.PlayerPrefsDisplayNameKey, DefaultPlayerName);
            CmdSetDisplayName(displayName);

            lobbyUI.SetActive(true);
        }

        [Command]
        private void CmdSetDisplayName(string displayName)
        {
            this.displayName = displayName;
        }

        [Server]
        public void SetIsHost(bool isHost)
        {
            IsHost = isHost;
        }

        public override void OnStartClient()
        {
            DontDestroyOnLoad(gameObject);
            NetworkManager.NetworkRoomPlayers.Add(this);
            UpdateDisplay();
        }

        public override void OnStopClient()
        {
            NetworkManager.NetworkRoomPlayers.Remove(this);
            UpdateDisplay();
        }

        private void OnDisplayNameChanged(string oldDisplayName, string newDisplayName) => UpdateDisplay();

        private void OnReadyStatusChanged(bool oldStatus, bool newStatus) => UpdateDisplay();

        private void UpdateDisplay()
        {
            if (!hasAuthority)
            {
                foreach (NetworkRoomPlayerMageBall player in NetworkManager.NetworkRoomPlayers)
                {
                    if (player.hasAuthority)
                    {
                        player.UpdateDisplay();
                        break;
                    }
                }

                return;
            }

            for (int i = 0; i < playerNameTexts.Length; i++)
            {
                playerNameTexts[i].text = "Waiting for player...";
                playerReadyTexts[i].text = "";
            }

            for (int i = 0; i < NetworkManager.NetworkRoomPlayers.Count; i++)
            {
                playerNameTexts[i].text = NetworkManager.NetworkRoomPlayers[i].DisplayName;
                playerReadyTexts[i].text = NetworkManager.NetworkRoomPlayers[i].IsReady ? "Ready" : "Not Ready";
            }
        }

        public void CheckIfReadyToStart(bool readyToStart)
        {
            if (!IsHost)
                return;

            startGameButton.Selectable = readyToStart;
        }

        [Command]
        public void CmdToggleReady()
        {
            isReady = !IsReady;
            NetworkManager.NotifyPlayersOfReadyState();
        }

        [Command]
        public void CmdStartGame()
        {
            if (NetworkManager.NetworkRoomPlayers[0].connectionToClient != connectionToClient)
                return;

            NetworkManager.StartGame();
        }

        public void Disconnect()
        {
            if (IsHost)
            {
                for (int i = NetworkManager.NetworkRoomPlayers.Count; i-- > 0;)
                {
                    if(NetworkManager.NetworkRoomPlayers[i] != this)
                        TargetDisconnect(NetworkManager.NetworkRoomPlayers[i].connectionToClient);
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
            yield return new WaitUntil(() => NetworkManager.NetworkRoomPlayers.Count == 1);

            NetworkManager.StopHost();
        }
    }
}