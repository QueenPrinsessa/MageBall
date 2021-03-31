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
        [SerializeField] private NetworkRoomPlayerMageBall networkRoomPlayerPrefab = null;

        public static event Action clientConnected;
        public static event Action clientDisconnected;

        public List<NetworkRoomPlayerMageBall> NetworkRoomPlayers { get; } = new List<NetworkRoomPlayerMageBall>();
        

        public override void OnClientConnect(NetworkConnection conn)
        {
            base.OnClientConnect(conn);
            clientConnected?.Invoke();
        }

        public override void OnClientDisconnect(NetworkConnection conn)
        {
            base.OnClientConnect(conn);
            clientDisconnected?.Invoke();
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
            if (numPlayers < minimumNumberOfPlayers)
                return false;

            foreach (NetworkRoomPlayerMageBall player in NetworkRoomPlayers)
                if (!player.IsReady)
                    return false;

            return true;
        }
    }
}