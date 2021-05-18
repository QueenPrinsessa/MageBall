using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

namespace MageBall
{
    public class ForceFly : LineSpell
    {

        [Header("Force Fly Settings")]
        [SerializeField] private GameObject forceFlyHitVFX;
        [SerializeField] private float durationInSeconds = 3;
        [SerializeField] private float upwardsForce = 10f;

        [Command]
        public override void CmdCastSpell()
        {
            TargetTriggerAttackAnimation("Attack2");

            if (Physics.SphereCast(aimPosition, HitRadius, aimForward, out RaycastHit hit, Range, LayerMasks.ballLayer | LayerMasks.propsLayer))
            {
                if (hit.rigidbody != null && !hit.rigidbody.isKinematic)
                {
                    hit.rigidbody.AddForce(Vector3.up * upwardsForce, ForceMode.Impulse);

                    if (hit.rigidbody.useGravity == true)
                    {
                        hit.rigidbody.useGravity = false;
                        StartCoroutine(EnableGravity(hit.rigidbody));
                        GameObject vfx = Instantiate(forceFlyHitVFX, hit.point, Quaternion.LookRotation(hit.normal));
                        FollowPosition followPosition = vfx.GetComponent<FollowPosition>();
                        if (followPosition != null)
                            followPosition.FollowTransform = hit.transform;
                        else
                            Debug.LogError("No FollowPosition script attached to Force Fly vfx");

                        NetworkServer.Spawn(vfx);
                        StartCoroutine(DestroyVFX(vfx, durationInSeconds));
                    }
                }
            }

            CreateLine(aimPosition, hit.point);
        }
        private IEnumerator EnableGravity(Rigidbody rigidbody)
        {
            yield return new WaitForSeconds(durationInSeconds);
            
            if(rigidbody != null)
                rigidbody.useGravity = true;
        }
    }
}
