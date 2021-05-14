using Mirror;
using System.Collections;
using UnityEngine;

namespace MageBall 
{
    public class PowerUp : NetworkBehaviour
    {

        [SerializeField] private int powerUpDuration = 5;

        public PowerUpSpawner Spawner { get; set; }


        [ServerCallback]
        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag(Tags.PlayerTag))
                return;

            Spellcasting spellcasting = other.gameObject.GetComponent<Spellcasting>();

            if (spellcasting == null)
                return;

            NetworkServer.Destroy(gameObject);
            spellcasting.TargetEnablePowerUp(powerUpDuration);
        }
    }
}
