using Mirror;
using System.Collections;
using UnityEngine;

namespace MageBall 
{
    public class PowerUp : NetworkBehaviour
    {

        [SerializeField] private int powerUpDuration = 5;
        private bool hasBeenPickedUp;

        public PowerUpSpawner Spawner { get; set; }


        [ServerCallback]
        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag(Tags.PlayerTag))
                return;

            Spellcasting spellcasting = other.gameObject.GetComponent<Spellcasting>();
            HUD hud = other.gameObject.GetComponent<HUD>();
            if (hasBeenPickedUp || spellcasting == null || hud == null)
                return;

            hasBeenPickedUp = true;
            NetworkServer.Destroy(gameObject);
            spellcasting.TargetEnablePowerUp(powerUpDuration);
            hud.TargetPowerUpManaBar(powerUpDuration);
        }
    }
}
