using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MageBall
{
    public class NetworkManagerMageBall : NetworkManager
    {
        [SerializeField] private int minimumNumberOfPlayers = 2;
        [SerializeField, Scene] private string menuScene = string.Empty;

        [Header("Room")]
        [SerializeField] private NetworkRoomPlayerMageBall networkRoomPlayerPrefab;

        [Header("Game")]
        [SerializeField] private NetworkGamePlayerMageBall gamePlayerPrefab;
        [SerializeField] private GameObject playerSpawnSystem;

        private string arenaPrefix = "Arena_";

        public static event Action clientConnected;
        public static event Action clientDisconnected;
        public static event Action stopClient;
        public static event Action<NetworkConnection> serverReadied;

        public List<NetworkRoomPlayerMageBall> NetworkRoomPlayers { get; } = new List<NetworkRoomPlayerMageBall>();
        public List<NetworkGamePlayerMageBall> NetworkGamePlayers { get; } = new List<NetworkGamePlayerMageBall>();

        public override void OnClientConnect(NetworkConnection conn)
        {
            if (!clientLoadedScene)
            {
                if (!NetworkClient.ready) NetworkClient.Ready();
                NetworkClient.AddPlayer();
            }

            clientConnected?.Invoke();
        }

        public override void OnClientDisconnect(NetworkConnection conn)
        {
            base.OnClientConnect(conn);
            clientDisconnected?.Invoke();
        }

        public override void OnStopClient()
        {
            stopClient?.Invoke();
        }

        public override void OnServerConnect(NetworkConnection conn)
        {
            if (numPlayers >= maxConnections)
            {
                conn.Disconnect();
                return;
            }

            //Stops players from joining if game is already in progress.
            if (SceneManager.GetActiveScene().path != menuScene)
            {
                conn.Disconnect();
                return;
            }

            NotifyPlayersOfReadyState();
        }

        public override void OnServerDisconnect(NetworkConnection conn)
        {
            if (conn.identity != null)
            {
                NetworkRoomPlayerMageBall player = conn.identity.GetComponent<NetworkRoomPlayerMageBall>();
                NetworkRoomPlayers.Remove(player);

                NotifyPlayersOfReadyState();
            }

            base.OnServerDisconnect(conn);
        }

        public override void OnServerAddPlayer(NetworkConnection conn)
        {
            if (SceneManager.GetActiveScene().path == menuScene)
            {
                bool isHost = NetworkRoomPlayers.Count == 0;

                NetworkRoomPlayerMageBall networkRoomPlayerInstance = Instantiate(networkRoomPlayerPrefab);
                networkRoomPlayerInstance.IsHost = isHost;
                NetworkServer.AddPlayerForConnection(conn, networkRoomPlayerInstance.gameObject);
            }
        }

        public override void OnStopServer()
        {
            NetworkRoomPlayers.Clear();
            base.OnStopServer();
        }

        public void NotifyPlayersOfReadyState()
        {
            foreach (NetworkRoomPlayerMageBall player in NetworkRoomPlayers)
                player.CheckIfReadyToStart(IsReadyToStartMatch());
        }

        private bool IsReadyToStartMatch()
        {
            //if (numPlayers < minimumNumberOfPlayers)
            //    return false;

            foreach (NetworkRoomPlayerMageBall player in NetworkRoomPlayers)
                if (!player.IsReady)
                    return false;

            return true;
        }

        public void StartGame()
        {
            if (SceneManager.GetActiveScene().path == menuScene)
            {
                if (!IsReadyToStartMatch())
                    return;

                ServerChangeScene("Arena_01");
            }
        }

        public override void ServerChangeScene(string newSceneName)
        {
            if (SceneManager.GetActiveScene().path == menuScene && newSceneName.StartsWith(arenaPrefix))
            {
                for (int i = NetworkRoomPlayers.Count; i-- > 0;)
                {
                    NetworkConnection connection = NetworkRoomPlayers[i].connectionToClient;
                    NetworkGamePlayerMageBall gamePlayerInstance = Instantiate(gamePlayerPrefab);
                    gamePlayerInstance.SetDisplayName(NetworkRoomPlayers[i].DisplayName);

                    NetworkServer.Destroy(connection.identity.gameObject);
                    NetworkServer.ReplacePlayerForConnection(connection, gamePlayerInstance.gameObject);
                }
            }

            base.ServerChangeScene(newSceneName);
        }

        public override void OnServerSceneChanged(string sceneName)
        {
            if (!sceneName.StartsWith(arenaPrefix))
                return;

            GameObject playerSpawnSystemInstance = Instantiate(playerSpawnSystem);
            NetworkServer.Spawn(playerSpawnSystemInstance);
        }

        public override void OnServerReady(NetworkConnection conn)
        {
            base.OnServerReady(conn);
            serverReadied?.Invoke(conn);
        }
    }
}