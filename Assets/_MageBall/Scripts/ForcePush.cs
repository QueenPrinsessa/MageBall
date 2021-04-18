using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MageBall
{
    public class ForcePush : Spell
    {

        [SerializeField] private GameObject forcePushPrefab;

        [Command]
        public override void CmdCastSpell()
        {
            GameObject projectile = Instantiate(forcePushPrefab, aimPosition, aimRotation);
            NetworkServer.Spawn(projectile);
        }
    }
}
