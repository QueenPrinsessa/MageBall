using Mirror;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MageBall
{
    public class PlayerSpawnSystem : NetworkBehaviour
    {
        [SerializeField] private GameObject redTeamPlayerPrefab;
        [SerializeField] private GameObject blueTeamPlayerPrefab;
        private static List<Transform> redSpawnPoints = new List<Transform>(); 
        private static List<Transform> blueSpawnPoints = new List<Transform>();

        private int nextIndex = 0;
        private Team currentTeam = Team.Red;

        public static void AddSpawnPoint(Team team, Transform transform)
        {
            switch (team)
            {
                case Team.Red:
                    redSpawnPoints.Add(transform);
                    redSpawnPoints = redSpawnPoints.OrderBy(x => x.GetSiblingIndex()).ToList();
                    break;
                case Team.Blue:
                    blueSpawnPoints.Add(transform);
                    blueSpawnPoints = blueSpawnPoints.OrderBy(x => x.GetSiblingIndex()).ToList();
                    break;
            }
        }

        public static void RemoveSpawnPoint(Team team, Transform transform)
        {
            switch (team)
            {
                case Team.Red:
                    redSpawnPoints.Remove(transform);
                    break;
                case Team.Blue:
                    blueSpawnPoints.Remove(transform);
                    break;
            }
        }

        public override void OnStartServer()
        {
            NetworkManagerMageBall.ServerReadied += OnServerReadied;
        }

        private void OnDestroy()
        {
            NetworkManagerMageBall.ServerReadied -= OnServerReadied;
        }

        [Server]
        public void OnServerReadied(NetworkConnection connection)
        {
            Transform spawnPoint;
            spawnPoint = (currentTeam == Team.Red) ? redSpawnPoints.ElementAtOrDefault(nextIndex) : blueSpawnPoints.ElementAtOrDefault(nextIndex);

            if (spawnPoint == null)
            {
                Debug.LogError($"There is no spawn point for player {nextIndex}!");
                return;
            }

            Vector3 position = (currentTeam == Team.Red) ? redSpawnPoints[nextIndex].position : blueSpawnPoints[nextIndex].position;
            Quaternion rotation = (currentTeam == Team.Red) ? redSpawnPoints[nextIndex].rotation : blueSpawnPoints[nextIndex].rotation;
            GameObject playerPrefab = (currentTeam == Team.Red) ? redTeamPlayerPrefab : blueTeamPlayerPrefab;

            GameObject playerInstance = Instantiate(playerPrefab, position, rotation);
            NetworkServer.Spawn(playerInstance, connection);

            NetworkGamePlayerMageBall networkGamePlayer = connection.identity.gameObject.GetComponent<NetworkGamePlayerMageBall>();
            networkGamePlayer.SetPlayerGameObject(playerInstance, position, rotation);
            networkGamePlayer.SetTeam(currentTeam);
            networkGamePlayer.TargetResetPlayerOwner();

            HUD hud = playerInstance.GetComponent<HUD>();
            if (hud != null)
                hud.SetNetworkGamePlayer(networkGamePlayer);

            if (currentTeam == Team.Blue)
            {
                currentTeam = Team.Red;
                nextIndex++;
            }
            else
                currentTeam = Team.Blue;
        }
    }
}
