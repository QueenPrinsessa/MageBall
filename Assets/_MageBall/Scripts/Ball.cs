using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

namespace MageBall
{
    /// <summary>
    /// Huvudansvarig: Gustaf
    /// </summary>
    public class Ball : NetworkBehaviour
    {
        [SerializeField] private float velocityLimit = 20f;
        [SerializeField] private new Rigidbody rigidbody;
        [SerializeField] private GameObject collisionVfx;
        [SerializeField] private AudioSource collisionSound;
        [SerializeField] private int collisionThreshold = 5;
        [SerializeField] private float vfxDuration = 1;

        [ServerCallback]
        private void OnCollisionEnter(Collision collision)
        {
            ContactPoint contact = collision.GetContact(0);

            if (collision.relativeVelocity.magnitude > collisionThreshold)
            {
                GameObject vfx = Instantiate(collisionVfx, contact.point, Quaternion.LookRotation(contact.normal));
                NetworkServer.Spawn(vfx);
                RpcPlaySound();
                StartCoroutine(DestroyAfterTime(vfx, vfxDuration));
            }
        }

        [ServerCallback]
        private void FixedUpdate()
        {
            if (rigidbody.isKinematic)
                return;

            if (rigidbody.velocity.sqrMagnitude > velocityLimit * velocityLimit)
            {
                Vector3 ballMoveDirection = rigidbody.velocity.normalized;
                rigidbody.velocity = velocityLimit * ballMoveDirection;
            }
        }

        [ClientRpc]
        private void RpcPlaySound()
        {
            collisionSound.Play();
        }

        [Server]
        protected IEnumerator DestroyAfterTime(GameObject vfx, float seconds)
        {
            yield return new WaitForSeconds(seconds);

            if (vfx != null)
                NetworkServer.Destroy(vfx);
        }
    }
}
