using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MageBall
{
    public class ForcePushProjectile : NetworkBehaviour
    {

        [SerializeField] private float pushSpeed = 10;
        
        private Vector3 pushLastPos;

        
        public override  void  OnStartServer()
        {
            Destroy(gameObject, 2f);
        }

        [Server]
        void Update()
        {
            pushLastPos = transform.position;
            transform.position += transform.forward * pushSpeed * Time.deltaTime;
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (!collision.gameObject.CompareTag(Tags.BallTag))
            {
                return;

            }

            Rigidbody rigidbody = collision.gameObject.GetComponent<Rigidbody>();
            if (rigidbody == null)
            {
                return;
            }

            ContactPoint contactPoint = collision.contacts[0];

            rigidbody.AddForce(-contactPoint.normal * pushSpeed, ForceMode.Impulse);
            Destroy(gameObject);


        }
    }
}