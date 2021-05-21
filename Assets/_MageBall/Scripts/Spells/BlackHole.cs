using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

namespace MageBall
{
    public class BlackHole : NetworkBehaviour
    {
        private const float GravitationalConstant = 6.672e-11f;

        [SerializeField] private float radius = 10f;
        [SerializeField] private float mass = 1.5e12f;
        [SerializeField] private float duration = 5f;
        [SerializeField] private float explosionForce = 2000f;

        public override void OnStartServer()
        {
            StartCoroutine(DestroyAfterTime(duration));
        }

        [ServerCallback]
        private void FixedUpdate()
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, radius, LayerMasks.ballLayer | LayerMasks.propsLayer);

            foreach (Collider collider in colliders)
            {
                Rigidbody rigidbody = collider.GetComponent<Rigidbody>();
                if (rigidbody == null)
                    continue;

                rigidbody.velocity += GAcceleration(transform.position, mass, rigidbody);
            }
        }

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

            float gravityForce = GravitationalConstant * ((mass * r.mass) / direction.sqrMagnitude);
            gravityForce /= r.mass;

            return direction.normalized * gravityForce * Time.fixedDeltaTime;
        }

        [Server]
        protected IEnumerator DestroyAfterTime(float seconds)
        {
            yield return new WaitForSeconds(seconds);

            Collider[] colliders = Physics.OverlapSphere(transform.position, radius, LayerMasks.ballLayer | LayerMasks.propsLayer);

            foreach (Collider collider in colliders)
            {
                Rigidbody rigidbody = collider.GetComponent<Rigidbody>();
                if (rigidbody == null)
                    continue;

                rigidbody.AddExplosionForce(explosionForce, transform.position, radius);
            }

            NetworkServer.Destroy(gameObject);
        }

    }
}