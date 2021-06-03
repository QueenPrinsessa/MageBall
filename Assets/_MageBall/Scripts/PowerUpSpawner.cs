using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

namespace MageBall
{
    /// <summary>
    /// Huvudansvarig: Gustaf
    /// </summary>
    public class PowerUpSpawner : NetworkBehaviour
    {

        [SerializeField] private int respawnRate = 60;
        [SerializeField] private GameObject powerUpPrefab;
        [SerializeField] private Transform spawnPosition;
        private GameObject currentPowerUp;

        public override void OnStartServer()
        {
            StartCoroutine(SpawnPowerUp());
        }

        [Server]
        private IEnumerator SpawnPowerUp()
        {
            yield return new WaitUntil(() => currentPowerUp == null);
            yield return new WaitForSeconds(respawnRate);

            currentPowerUp = Instantiate(powerUpPrefab, spawnPosition.position, spawnPosition.rotation);
            PowerUp powerUp = currentPowerUp.GetComponent<PowerUp>();
            powerUp.Spawner = this;
            NetworkServer.Spawn(currentPowerUp);
            StartCoroutine(SpawnPowerUp());

        }
    }
}
