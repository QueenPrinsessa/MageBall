using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

namespace MageBall
{
    public class ForceFly : Spell
    {

        [SerializeField] private float hitRadius = 0.2f;
        [SerializeField] private GameObject forceFlyHitVFX;
        [SerializeField] private float durationInSeconds = 5;

        [Command]
        public override void CmdCastSpell()
        {
            TargetTriggerAttackAnimation("Attack2");

            if (Physics.SphereCast(aimPosition, hitRadius, aimForward, out RaycastHit hit, Mathf.Infinity, LayerMasks.ballLayer))
            {
                if (hit.rigidbody != null)
                {
                    if(hit.rigidbody.useGravity == true)
                    {
                        hit.rigidbody.useGravity = false;
                        StartCoroutine(EnableGravity());
                        GameObject vfx = Instantiate(forceFlyHitVFX, hit.point, Quaternion.LookRotation(hit.normal));
                        vfx.AddComponent<FollowPosition>().FollowTransform = hit.transform;
                        NetworkServer.Spawn(vfx);
                        StartCoroutine(DestroyVFX(vfx, durationInSeconds));
                    }
                }
            }
        }
        private IEnumerator EnableGravity()
        {
            yield return new WaitForSeconds(durationInSeconds);
            GameObject ball = GameObject.FindGameObjectWithTag(Tags.BallTag);
            Rigidbody rigidbody = ball.GetComponent<Rigidbody>();
            rigidbody.useGravity = true;
        }
    }
}
