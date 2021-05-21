using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MageBall
{
    public class ForcePushProjectile : NetworkBehaviour
    {

        [SerializeField] private new Rigidbody rigidbody;
        [SerializeField] private new Collider collider;
        [SerializeField] private float flightSpeed = 20;
        [SerializeField] private float explosionRadius = 4f;
        [SerializeField] private float explosionForce = 2000f;
        [SerializeField] private float vfxDuration = 3f;
        [SerializeField] private GameObject forcePushHitVFX;
        [SerializeField] private AudioSource audioSource;

        [ServerCallback]
        private void FixedUpdate()
        {
            Vector3 moveDelta = transform.forward * flightSpeed * Time.fixedDeltaTime;
            rigidbody.MovePosition(transform.position + moveDelta);
        }

        [ServerCallback]
        private void OnCollisionEnter(Collision collision)
        {
            ContactPoint contact = collision.GetContact(0);

            Collider[] colliders = Physics.OverlapSphere(contact.point, explosionRadius, LayerMasks.propsLayer | LayerMasks.ballLayer | LayerMasks.groundLayer);

            foreach (Collider collider in colliders)
            {
                if (collider.attachedRigidbody == null)
                    continue;

                collider.attachedRigidbody.AddExplosionForce(explosionForce, contact.point, explosionRadius);
            }

            if (colliders.Length > 0)
            {
                audioSource.Stop();
                collider.enabled = false;
                RpcDisableProjectileVFX();
                GameObject vfx = Instantiate(forcePushHitVFX, contact.point, Quaternion.LookRotation(contact.normal));
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