using Cinemachine;
using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MageBall
{
    public class PlayerMovement : NetworkBehaviour
    {
        private CharacterController controller;
        private Animator animator;
        private float speed = 0f;
        [SerializeField] private float maxSpeed = 10f;
        [SerializeField] private float forceMagnitude = 6f;
        [SerializeField] private float jumpHeight = 1.3f;
        [SerializeField] private float gravity = -10.0f;
        [SerializeField] private Passive speedPassive;
        [SerializeField] private Passive jumpPassive;
        private float groundCheckDistance = 0.25f;
        private float groundCheckRadius = 0.25f;
        private Vector3 moveDirection;
        private Vector3 velocity;

        public override void OnStartAuthority()
        {
            controller = GetComponent<CharacterController>();
            animator = GetComponent<Animator>();
        }

        private void OnDisable()
        {
            if (!hasAuthority)
                return;
            animator.SetFloat("Speed", 0f);
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

            if (horizontal != 0 || vertical != 0)
            {
                speed = Mathf.Min(speed + forceMagnitude * Time.deltaTime, maxSpeed * speedPassive.modifier);

                if (vertical < 0)
                    animator.SetBool("RunBackward", true);
                else
                    animator.SetBool("RunBackward", false);
            }
            else
                speed = 0f;

            animator.SetFloat("Speed", speed);

            Vector3 flatMovement = speed * Time.deltaTime * transformDirection;

            moveDirection = new Vector3(flatMovement.x, moveDirection.y, flatMovement.z);
            controller.Move(moveDirection);

            bool isGrounded = IsGrounded();

            if (isGrounded)
            {
                moveDirection.y = 0;
                velocity.y = 0;
                animator.SetBool("IsJumping", false);
            }

            if (Input.GetButton("Jump") && isGrounded)
            {
                velocity.y += Mathf.Sqrt((jumpHeight * jumpPassive.modifier) * -3.0f * gravity);
                animator.SetBool("IsJumping", true);
            }

            velocity.y += gravity * Time.deltaTime;
            controller.Move(velocity * Time.deltaTime);
        }


        private bool IsGrounded()
        {
            Vector3 groundCheckPosition = transform.position + transform.up * (groundCheckRadius + Physics.defaultContactOffset);
            Physics.SphereCast(groundCheckPosition, groundCheckRadius, -transform.up, out RaycastHit raycastHit, groundCheckDistance);
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
            Rigidbody rigidbody = go.GetComponent<Rigidbody>();

            if (rigidbody == null || rigidbody.isKinematic)
                return;

            if (moveDirection.y < -0.3)
                return;

            Vector3 pushDirection = new Vector3(moveDirection.x, 0, moveDirection.z);

            rigidbody.velocity = pushDirection * speed * 1.5f;
        }

    }
}