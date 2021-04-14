﻿using Cinemachine;
using Mirror;
using System.Collections.Generic;
using UnityEngine;

namespace MageBall
{
    public class PlayerMovement : NetworkBehaviour
    {
        private CharacterController controller;
        private float speed = 0f;
        [SerializeField] private float maxSpeed = 10f;
        [SerializeField] private float forceMagnitude = 4f; 
        [SerializeField] private float jumpHeight = 0.9f;
        [SerializeField] private float gravity = -10.0f;
        private float groundCheckDistance = 0.25f;
        private Vector3 moveDirection;
        private Vector3 velocity;

        public override void OnStartAuthority()
        {
            controller = GetComponent<CharacterController>();
        }

        [Client]
        private void FixedUpdate()
        {
            if (!hasAuthority)
                return;

            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");

            Vector3 inputDirection = new Vector3(horizontal, 0, vertical);
            Vector3 transformDirection = transform.TransformDirection(inputDirection);
            
            if(horizontal != 0 || vertical != 0)
            {
                speed = Mathf.Min(speed + forceMagnitude * Time.deltaTime, maxSpeed);
            }
            else
            {
                speed = Mathf.Max(speed - forceMagnitude * Time.deltaTime * 1.5f, 0);
            }

             //Debug.Log(speed);
           
            Vector3 flatMovement = speed * Time.deltaTime * transformDirection;

            moveDirection = new Vector3(flatMovement.x, moveDirection.y, flatMovement.z);
            controller.Move(moveDirection);

            bool isGrounded = IsGrounded();

            if (isGrounded)
            {
                moveDirection.y = 0;
                velocity.y = 0;
            }

            if (Input.GetButton("Jump") && isGrounded)
                velocity.y += Mathf.Sqrt(jumpHeight * -3.0f * gravity);

            velocity.y += gravity * Time.deltaTime;
            controller.Move(velocity * Time.deltaTime);
        }

        private Color rayColor;

        private bool IsGrounded()
        {
            Physics.Raycast(new Ray(transform.position, -transform.up), out RaycastHit raycastHit, groundCheckDistance);

#if UNITY_EDITOR
            if(raycastHit.collider != null) // if the raycast is not null it has hit something
            {
                rayColor = Color.green; // green for grounded
            }
            else
            {
                rayColor = Color.red; // red for floating?
            }
#endif
            return raycastHit.collider != null;
        }

        [Client]
        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            if (hit.rigidbody == null || hit.rigidbody.isKinematic)
                return;

            CmdPushRigidbody(hit.gameObject, hit.moveDirection);
        }

        [Command]
        private void CmdPushRigidbody(GameObject go, Vector3 moveDirection)
        {
            Rigidbody rigidbody = go.GetComponent<Rigidbody>(); //.velocity = push;

            if (rigidbody == null || rigidbody.isKinematic)
                return;

            if (moveDirection.y < -0.3)
                return;

            Vector3 pushDirection = new Vector3(moveDirection.x, 0, moveDirection.z);

            rigidbody.velocity = pushDirection * speed * 1.5f;
        }


        private void OnDrawGizmos()
        {
            if (rayColor == null)
                return;

            Gizmos.color = rayColor;
            Ray groundCheckRay = new Ray(transform.position, -transform.up);
            Gizmos.DrawLine(groundCheckRay.origin, (groundCheckRay.direction.normalized * groundCheckDistance) + groundCheckRay.origin);
        }

    }
}