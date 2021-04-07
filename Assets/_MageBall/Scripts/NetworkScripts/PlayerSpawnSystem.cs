using Mirror;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MageBall
{
    public class PlayerSpawnSystem : NetworkBehaviour
    {
        [SerializeField] private GameObject playerPrefab;
        private static List<Transform> redSpawnPoints = new List<Transform>(); // Change into red & blue player spawns
        private static List<Transform> blueSpawnPoints = new List<Transform>(); // Change into red & blue player spawns

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
            NetworkManagerMageBall.serverReadied += OnServerReadied;
        }

        [ServerCallback]
        private void OnDestroy()
        {
            NetworkManagerMageBall.serverReadied -= OnServerReadied;
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

            GameObject playerInstance = Instantiate(playerPrefab, position, rotation);
            NetworkServer.Spawn(playerInstance, connection);

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
