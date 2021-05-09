using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MageBall
{
    [RequireComponent(typeof(CharacterController))]
    public class CharacterControllerGravity : NetworkBehaviour
    {
        [Header("Settings")]
        [SerializeField, SyncVar] private float gravity = -10.0f;
        [SerializeField] private float groundCheckDistance = 0.25f;

        private CharacterController controller;
        private Vector3 velocity;

        public float Gravity => gravity;

        public override void OnStartAuthority()
        {
            controller = GetComponent<CharacterController>();
        }

        [Client]
        private void FixedUpdate()
        {
            if (!hasAuthority)
                return;

            if (IsGrounded())
                velocity.y = 0;

            velocity.y += gravity * Time.deltaTime;
            controller.Move(velocity * Time.deltaTime);
        }

        public bool IsGrounded()
        {
            Physics.Raycast(transform.position, -transform.up, out RaycastHit raycastHit, groundCheckDistance);
            return raycastHit.collider != null;
        }
    }
}