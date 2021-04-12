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
            GameObject projectile = Instantiate(forcePushPrefab, castPoint.position, castPoint.rotation);
            NetworkServer.Spawn(projectile);
        }
    }
}
