using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MageBall
{
    public class ForcePushProjectile : NetworkBehaviour
    {

        [SerializeField] private float flightSpeed = 20;
        [SerializeField] private float radius = 2f;
        [SerializeField] private float explosionForce = 2000f;
        [SerializeField] private float vfxDuration = 3f;
        [SerializeField] private GameObject forcePushHitVFX;

        [Server]
        void Update()
        {
            transform.Translate(transform.forward * flightSpeed * Time.deltaTime, Space.World);
        }

        [Server]
        private void OnCollisionEnter(Collision collision)
        {
            Vector3 explosionPoint = transform.position;
            Collider[] colliders = Physics.OverlapSphere(explosionPoint, radius, LayerMasks.ballLayer | LayerMasks.groundLayer);

            foreach (Collider collider in colliders)
            {
                Rigidbody rigidbody = collider.GetComponent<Rigidbody>();
                if (rigidbody == null)
                    continue;

                rigidbody.AddExplosionForce(explosionForce, explosionPoint, radius);
            }

            if (colliders.Length > 0)
            {
                gameObject.GetComponent<Collider>().enabled = false;
                GameObject vfx = Instantiate(forcePushHitVFX,explosionPoint, Quaternion.LookRotation(transform.position));
                NetworkServer.Spawn(vfx);
                StartCoroutine(DestroyAfterTime(vfx, vfxDuration));
            }
        }

        [Server]
        protected IEnumerator DestroyAfterTime(GameObject vfx, float seconds)
        {
            yield return new WaitForSeconds(seconds);

            if (vfx != null)
                NetworkServer.Destroy(vfx);

            NetworkServer.Destroy(gameObject);
        }
    }
}