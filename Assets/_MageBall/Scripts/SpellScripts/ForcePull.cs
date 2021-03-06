using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
namespace MageBall
{

    public class ForcePull : LineSpell
    {
        [Header("Force Pull Settings")]
        [SerializeField] private float force = 40.0f;
        [SerializeField] private GameObject forcePullHitVFX;
        [SerializeField] private float vfxDuration = 1.5f;

        [Command]
        public override void CmdCastSpell()
        {
            TargetTriggerAttackAnimation("Attack2");

            if (Physics.SphereCast(aimPosition, HitRadius, aimForward, out RaycastHit hit, Range, LayerMasks.ballLayer | LayerMasks.propsLayer))
            {
                Vector3 pullDirection = aimPosition - hit.transform.position;
                Vector3 pullForce = pullDirection.normalized * force;
                if (hit.rigidbody != null)
                {
                    hit.rigidbody.AddForce(pullForce, ForceMode.Impulse);
                    GameObject vfx= Instantiate(forcePullHitVFX, hit.point, Quaternion.LookRotation(hit.normal));
                    VFXFollowObject followPosition = vfx.GetComponent<VFXFollowObject>();
                    if (followPosition != null)
                        followPosition.FollowTransform = hit.transform;
                    else
                        Debug.LogError("No FollowPosition script attached to ForcePull vfx");
                    NetworkServer.Spawn(vfx);
                    StartCoroutine(DestroyVFX(vfx, vfxDuration));
                }
            }

            CreateLine(aimPosition, hit.point);
        }

    }
}