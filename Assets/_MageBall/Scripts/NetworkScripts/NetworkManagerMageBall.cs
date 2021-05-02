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

        [Header("Settings")]
        [SerializeField, Tooltip("The time in seconds before a player gains control of their character at the start of the game and after goals.")] 
        private float waitBeforeControlsEnableInSeconds = 3;
        [SerializeField, Tooltip("The time in seconds before a reset occurs after a goal.")]
        private float waitBeforeResetAfterGoalInSeconds = 1.5f;
        [SerializeField, Tooltip("The time in seconds before players are returned to lobby.")]
        private float waitBeforeReturnToLobbyInSeconds = 3f;
        [SerializeField]
        private int matchLengthInMinutes = 5;

        private string arenaPrefix = "Arena_";

        public static event Action ClientConnected;
        public static event Action ClientDisconnected;
        public static event Action StopClientEvent; //Named like that to avoid conflict with StopClient() method
        public static event Action<NetworkConnection> ServerReadied;

        public List<NetworkRoomPlayerMageBall> NetworkRoomPlayers { get; } = new List<NetworkRoomPlayerMageBall>();
        public List<NetworkGamePlayerMageBall> NetworkGamePlayers { get; } = new List<NetworkGamePlayerMageBall>();
        public float WaitBeforeControlsEnableInSeconds => waitBeforeControlsEnableInSeconds;
        public float WaitBeforeResetAfterGoalInSeconds => waitBeforeResetAfterGoalInSeconds;
        public float WaitBeforeReturnToLobbyInSeconds => waitBeforeReturnToLobbyInSeconds;
        public int MatchLength => matchLengthInMinutes;

        public override void OnClientConnect(NetworkConnection conn)
        {
            if (!clientLoadedScene)
            {
                if (!NetworkClient.ready) NetworkClient.Ready();
                NetworkClient.AddPlayer();
            }

            ClientConnected?.Invoke();
        }

        public override void OnClientDisconnect(NetworkConnection conn)
        {
            base.OnClientConnect(conn);
            ClientDisconnected?.Invoke();
        }

        public override void OnStopClient()
        {
            StopClientEvent?.Invoke();
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
                player.IsLobbyReady(IsReadyToStartMatch());
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

                ServerChangeScene(Scenes.Arena01);
            }
        }

        public void ReturnToLobby()
        {
            ServerChangeScene(Scenes.MainMenu);
        }

        public void ServerChangeScene(Scenes scene)
        {
            ServerChangeScene(GetSceneName(scene));
        }

        private string GetSceneName(Scenes scene)
        {
            string scenePath = SceneUtility.GetScenePathByBuildIndex((int)scene);
            string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);
            return sceneName;
        }

        public override void ServerChangeScene(string newSceneName)
        {
            if (SceneManager.GetActiveScene().path == menuScene && newSceneName.StartsWith(arenaPrefix))
            {
                RoomToGame();
            }
            else if (newSceneName == GetSceneName(Scenes.MainMenu))
            {
                GameToRoom();
            }

            base.ServerChangeScene(newSceneName);
        }

        private void GameToRoom()
        {
            for (int i = NetworkGamePlayers.Count; i-- > 0;)
            {
                NetworkConnection connection = NetworkGamePlayers[i].connectionToClient;
                NetworkRoomPlayerMageBall roomPlayerInstance = Instantiate(networkRoomPlayerPrefab);
                roomPlayerInstance.SetIsHost(NetworkGamePlayers[i].IsHost);

                NetworkServer.Destroy(connection.identity.gameObject);
                NetworkServer.ReplacePlayerForConnection(connection, roomPlayerInstance.gameObject);
            }
        }

        private void RoomToGame()
        {
            for (int i = NetworkRoomPlayers.Count; i-- > 0;)
            {
                NetworkConnection connection = NetworkRoomPlayers[i].connectionToClient;
                NetworkGamePlayerMageBall gamePlayerInstance = Instantiate(gamePlayerPrefab);
                gamePlayerInstance.SetDisplayName(NetworkRoomPlayers[i].DisplayName);
                gamePlayerInstance.SetIsHost(NetworkRoomPlayers[i].IsHost);

                NetworkServer.Destroy(connection.identity.gameObject);
                NetworkServer.ReplacePlayerForConnection(connection, gamePlayerInstance.gameObject);
            }
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
            ServerReadied?.Invoke(conn);
        }

    }
}