using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MageBall
{
    public class Minimize : LineSpell
    {
        [Header("Minimize Settings")]
        [SerializeField] private float modifier = 0.8f;
        [SerializeField] private GameObject minimizeHitVFX;
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
                    resizable.ApplySpell(new SpellStackInfo(Spells.Minimize, modifier, duration));
                    //Spawn VFX:
                    //GameObject vfx = Instantiate(minimizeHitVFX, hit.transform.position, Quaternion.identity);
                    //FollowPosition followPosition = vfx.GetComponent<FollowPosition>();
                    //if (followPosition != null)
                    //    followPosition.FollowTransform = hit.transform;
                    //else
                    //    Debug.LogError("No FollowPosition script attached to Minimize vfx");
                    //NetworkServer.Spawn(vfx);
                    //StartCoroutine(DestroyVFX(vfx, vfxDuration));
                }
            }

            CreateLine(aimPosition, hit.point);
        }

    }
}