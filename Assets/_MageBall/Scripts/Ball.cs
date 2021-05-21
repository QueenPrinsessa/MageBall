using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

namespace MageBall
{
    public class Ball : NetworkBehaviour
    {
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
