using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MageBall
{
    public class MagnifySpell : LineSpell
    {
        [Header("Magnify Settings")]
        [SerializeField] private int stacks;
        [SerializeField] private GameObject magnifyHitVFX;
        [SerializeField] private float vfxDuration = 1.5f;

        [Command]
        public override void CmdCastSpell()
        {
            TargetTriggerAttackAnimation("Attack2");

            if (Physics.SphereCast(aimPosition, HitRadius, aimForward, out RaycastHit hit, Range, LayerMasks.ballLayer | LayerMasks.propsLayer))
            {


                //Spawn VFX:
                //GameObject vfx = Instantiate(forcePullHitVFX, hit.point, Quaternion.LookRotation(hit.normal));
                //FollowPosition followPosition = vfx.GetComponent<FollowPosition>();
                //if (followPosition != null)
                //    followPosition.FollowTransform = hit.transform;
                //else
                //    Debug.LogError("No FollowPosition script attached to ForcePull vfx");
                //NetworkServer.Spawn(vfx);
                //StartCoroutine(DestroyVFX(vfx, vfxDuration));
            }

            CreateLine(aimPosition, hit.point);
        }

    }
}