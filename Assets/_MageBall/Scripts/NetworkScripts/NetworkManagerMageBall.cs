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
        [SerializeField, Scene] private string menuScene = string.Empty;
        [Header("Room")]
        [SerializeField] private NetworkRoomPlayerMageBall networkRoomPlayerPrefab = null;

        public static event Action clientConnected;
        public static event Action clientDisconnected;

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
            if (SceneManager.GetActiveScene().path == menuScene)
            {
                conn.Disconnect();
                return;
            }
        }

        public override void OnServerAddPlayer(NetworkConnection conn)
        {
            if (SceneManager.GetActiveScene().path == menuScene)
            {
                NetworkRoomPlayerMageBall networkRoomPlayerInstance = Instantiate(networkRoomPlayerPrefab);
                NetworkServer.AddPlayerForConnection(conn, networkRoomPlayerInstance.gameObject);
            }
        }
    }
}