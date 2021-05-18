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

        [ServerCallback]
        void Update()
        {
            transform.Translate(transform.forward * flightSpeed * Time.deltaTime, Space.World);
        }

        [ServerCallback]
        private void OnCollisionEnter(Collision collision)
        {
            Vector3 explosionPoint = transform.position;
            Collider[] colliders = Physics.OverlapSphere(explosionPoint, radius, LayerMasks.propsLayer | LayerMasks.ballLayer | LayerMasks.groundLayer);

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
                RpcDisableProjectileVFX();
                ContactPoint contact = collision.GetContact(0);
                GameObject vfx = Instantiate(forcePushHitVFX, explosionPoint, Quaternion.LookRotation(contact.normal));
                NetworkServer.Spawn(vfx);
                StartCoroutine(DestroyAfterTime(vfx, vfxDuration));
            }
        }

        [ClientRpc]
        private void RpcDisableProjectileVFX()
        {
            gameObject.transform.GetChild(0).gameObject.SetActive(false);
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