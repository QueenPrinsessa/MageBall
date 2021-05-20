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

        private void OnCollisionEnter(Collision collision)
        {
            ContactPoint contact = collision.contacts[0];
            Quaternion rotation = Quaternion.LookRotation(contact.normal);
            Vector3 position = contact.point;

            if(collision.relativeVelocity.magnitude > collisionThreshold)
            {
                GameObject vfx = Instantiate(collisionVfx, position, rotation);
                NetworkServer.Spawn(vfx);
                collisionSound.Play();
                StartCoroutine(DestroyAfterTime(vfx, vfxDuration));
            }
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
