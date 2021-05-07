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

        private bool IsGrounded()
        {
            Vector3 groundCheckPosition = transform.position + transform.up * (0.25f + Physics.defaultContactOffset);
            Physics.SphereCast(groundCheckPosition, 0.25f, -transform.up, out RaycastHit raycastHit, 0.25f);
            return raycastHit.collider != null;
        }
    }
}