using Mirror;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MageBall
{
    /// <summary>
    /// Huvudansvaring: Zoe 
    /// </summary>
    public class PlayerSpawnSystem : NetworkBehaviour
    {
        [Header("Male player prefabs")]
        [SerializeField] private GameObject redTeamMalePlayerPrefab;
        [SerializeField] private GameObject blueTeamMalePlayerPrefab;
        [Header("Female player prefabs")]
        [SerializeField] private GameObject redTeamFemalePlayerPrefab;
        [SerializeField] private GameObject blueTeamFemalePlayerPrefab;
        private static List<Transform> redSpawnPoints = new List<Transform>(); 
        private static List<Transform> blueSpawnPoints = new List<Transform>();

        private int nextIndex = 0;
        private Team currentTeam;
        private Team firstTeam;

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
            //0 = Team.Red
            //1 = Team.Blue
            currentTeam = (Team) Random.Range(0, 2);
            firstTeam = currentTeam;
            NetworkManagerMageBall.ServerReadied += OnServerReadied;
        }

        private void OnDestroy()
        {
            NetworkManagerMageBall.ServerReadied -= OnServerReadied;
        }

        [Server]
        public void OnServerReadied(NetworkConnection connection)
        {
            NetworkGamePlayerMageBall networkGamePlayer = connection.identity.gameObject.GetComponent<NetworkGamePlayerMageBall>();

            Transform spawnPoint;
            spawnPoint = (currentTeam == Team.Red) ? redSpawnPoints.ElementAtOrDefault(nextIndex) : blueSpawnPoints.ElementAtOrDefault(nextIndex);

            if (spawnPoint == null)
            {
                Debug.LogError($"There is no spawn point for player {nextIndex}!");
                return;
            }

            Vector3 position = (currentTeam == Team.Red) ? redSpawnPoints[nextIndex].position : blueSpawnPoints[nextIndex].position;
            Quaternion rotation = (currentTeam == Team.Red) ? redSpawnPoints[nextIndex].rotation : blueSpawnPoints[nextIndex].rotation;
            GameObject playerPrefab = GetPlayerPrefabFromLoadout(networkGamePlayer.PlayerLoadout);

            GameObject playerInstance = Instantiate(playerPrefab, position, rotation);
            NetworkServer.Spawn(playerInstance, connection);

            networkGamePlayer.SetPlayerGameObject(playerInstance, position, rotation);
            networkGamePlayer.TargetResetPlayerOwner();

            HUD hud = playerInstance.GetComponent<HUD>();
            hud.SetNetworkGamePlayerMageBall(networkGamePlayer);

            Spellcasting spellcasting = playerInstance.GetComponent<Spellcasting>();
            spellcasting.SetPlayerLoadout(networkGamePlayer.PlayerLoadout);

            PlayerMovement playerMovement = playerInstance.GetComponent<PlayerMovement>();
            playerMovement.SetPassiveFromLoadout(networkGamePlayer.PlayerLoadout);

            PlayerNameTag playerNameTag = playerInstance.GetComponent<PlayerNameTag>();
            playerNameTag.SetPlayerName(networkGamePlayer.DisplayName);

            if (currentTeam != firstTeam)
                nextIndex++;

            if (currentTeam == Team.Blue)
                currentTeam = Team.Red;
            else
                currentTeam = Team.Blue;
        }

        [Server]
        private GameObject GetPlayerPrefabFromLoadout(PlayerLoadout playerLoadout)
        {
            switch (playerLoadout.PlayerModel)
            {
                case PlayerModel.Man:
                    return (currentTeam == Team.Red) ? redTeamMalePlayerPrefab : blueTeamMalePlayerPrefab;
                case PlayerModel.Woman:
                    return (currentTeam == Team.Red) ? redTeamFemalePlayerPrefab : blueTeamFemalePlayerPrefab;
                default:
                    return redTeamMalePlayerPrefab;
            }
        }
    }
}
