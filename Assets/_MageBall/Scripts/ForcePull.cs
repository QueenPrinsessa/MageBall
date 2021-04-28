using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
namespace MageBall
{

    public class ForcePull : Spell
    {

        [SerializeField] private float modifier = 15.0f;
        [SerializeField] private float hitRadius = 0.2f;
        [SerializeField] private GameObject forcePullHitVFX;
        
        [Command]
        public override void CmdCastSpell()
        {
            if (Physics.SphereCast(aimPosition, hitRadius, aimForward, out RaycastHit hit, Mathf.Infinity, LayerMasks.ballLayer))
            {
                Vector3 pullDirection = aimPosition - hit.transform.position;
                Vector3 pullForce = pullDirection.normalized * modifier;
                if (hit.rigidbody != null)
                {
                    hit.rigidbody.AddForce(pullForce, ForceMode.Impulse);
                    GameObject vfx= Instantiate(forcePullHitVFX, hit.point, Quaternion.LookRotation(hit.normal));
                    vfx.transform.parent = hit.transform;
                    NetworkServer.Spawn(vfx);
                }
            }
        }
    }
}