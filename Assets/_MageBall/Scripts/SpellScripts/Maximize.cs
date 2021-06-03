using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MageBall
{
    public class Maximize : LineSpell
    {
        [Header("Maximize Settings")]
        [SerializeField] private float modifier = 1.2f;
        [SerializeField] private GameObject maximizeHitVFX;
        [SerializeField] private float duration = 10f;
        [SerializeField] private float vfxDuration = 10f;

        [Command]
        public override void CmdCastSpell()
        {
            TargetTriggerAttackAnimation("Attack2");

            if (Physics.SphereCast(aimPosition, HitRadius, aimForward, out RaycastHit hit, Range, LayerMasks.ballLayer | LayerMasks.propsLayer))
            {
                Resizable resizable = hit.transform.GetComponent<Resizable>();
                if (resizable != null)
                {
                    resizable.ApplySpell(new SpellStackInfo(Spells.Maximize, modifier, vfxDuration));


                    if (maximizeHitVFX != null)
                    {
                        GameObject vfx = Instantiate(maximizeHitVFX, hit.transform.position, Quaternion.identity);
                        VFXFollowObject followPosition = vfx.GetComponent<VFXFollowObject>();
                        if (followPosition != null)
                            followPosition.FollowTransform = hit.transform;
                        else
                            Debug.LogError("No FollowPosition script attached to Maximize vfx");
                        NetworkServer.Spawn(vfx);
                        StartCoroutine(DestroyVFX(vfx, vfxDuration));
                    }
                }
            }

            CreateLine(aimPosition, hit.point);
        }

    }
}