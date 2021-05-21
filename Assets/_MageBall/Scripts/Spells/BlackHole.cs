using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

namespace MageBall
{
    public class BlackHole : LineSpell
    {
        [Header("Black Hole Settings")]
        [SerializeField] private GameObject blackHoleVfx;
        [SerializeField] private float spellDuration = 3f;
        [SerializeField] private float mass = 100000;

        [Command]
        public override void CmdCastSpell()
        {
            TargetTriggerAttackAnimation("Attack1");
            if (Physics.Raycast(aimPosition, aimForward, out RaycastHit hit, Range, LayerMasks.ballLayer | LayerMasks.propsLayer | LayerMasks.groundLayer))
            {
                Collider[] colliders = Physics.OverlapSphere(hit.point, HitRadius, LayerMasks.ballLayer | LayerMasks.propsLayer);

                StartCoroutine(UpdateGravity(colliders, hit.point));

                //GameObject vfx = Instantiate(blackHoleVfx, hit.point, Quaternion.LookRotation(hit.normal));
                //NetworkServer.Spawn(vfx);
            }

            CreateLine(aimPosition, hit.point);
            
        }

        [Server]
        protected IEnumerator DestroyAfterTime(GameObject vfx, float seconds)
        {
            yield return new WaitForSeconds(seconds);

            if (vfx != null)
                NetworkServer.Destroy(vfx);

            NetworkServer.Destroy(gameObject);
        }

        private const float gravitationalConstant = 6.672e-11f;

        /// <summary>
        /// Calculates gravitational acceleration for a Rigidbody under the influence of a large "mass" point at a "position".
        /// Use this in each FixedUpdate(): rigidbody.velocity += GAcceleration(planetPosition, planetMass, rigidbody);
        /// </summary>
        /// <returns>The acceleration.</returns>
        /// <param name="position">Position of the planet's center of mass.</param>
        /// <param name="mass">Mass of the planet (kg). Use large values. </param>
        /// <param name="r">The Rigidbody to accelerate.</param>
        public static Vector3 GAcceleration(Vector3 position, float mass, Rigidbody r)
        {
            Vector3 direction = position - r.position;

            float gravityForce = gravitationalConstant * ((mass * r.mass) / direction.sqrMagnitude);
            gravityForce /= r.mass;

            return direction.normalized * gravityForce * Time.fixedDeltaTime;
        }

        private IEnumerator UpdateGravity(Collider[] colliders, Vector3 blackHolePos)
        {
            bool isActive = true;
            while (isActive)
            {
                yield return new WaitForFixedUpdate();

                foreach (Collider collider in colliders)
                {
                    Rigidbody rigidbody = collider.GetComponent<Rigidbody>();
                    if (rigidbody == null)
                        continue;

                    rigidbody.velocity += GAcceleration(blackHolePos, mass, rigidbody);
                }
            }
        }
    }
}
