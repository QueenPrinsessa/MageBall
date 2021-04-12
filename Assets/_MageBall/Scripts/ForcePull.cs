using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
namespace MageBall
{

    public class ForcePull : Spell
    {

        [SerializeField]
        private float modifier = 10.0f;

        [Command]
        public override void CmdCastSpell()
        {
            if (Physics.Raycast(castPoint.position, castPoint.forward, out RaycastHit hit, Mathf.Infinity, LayerMasks.ballLayer))
            {
                Vector3 pullDirection = castPoint.position - hit.transform.position;
                Vector3 pullForce = pullDirection.normalized * modifier;
                if (hit.rigidbody != null)
                    hit.rigidbody.AddForce(pullForce, ForceMode.Impulse);
            }
        }
    }
}