using Mirror;
using System;
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
        [SerializeField] private float groundCheckDistance = 0.25f;
        [SerializeField] private float groundCheckRadius = 0.25f;
        [SyncVar] private Passives currentPassive;
        private Vector3 moveDirection;
        private Vector3 velocity;

        private float JumpHeight => currentPassive == Passives.JumpBoost ? jumpHeight * jumpPassive.modifier : jumpHeight;
        private float MaxSpeed => currentPassive == Passives.SpeedBoost ? maxSpeed * speedPassive.modifier : maxSpeed;
        public override void OnStartAuthority()
        {
            controller = GetComponent<CharacterController>();
            animator = GetComponent<Animator>();
        }

        [Server]
        public void SetPassiveFromLoadout(PlayerLoadout playerLoadout)
        {
            currentPassive = playerLoadout.Passive;
        }

        [Client]
        private void FixedUpdate()
        {
            if (!hasAuthority)
                return;

            HandleMovement();

            bool isGrounded = IsGrounded();

            if (isGrounded)
            {
                moveDirection.y = 0;
                velocity.y = 0;
                animator.SetBool("IsJumping", false);
            }

            if (Input.GetButton("Jump") && isGrounded)
            {
                velocity.y += Mathf.Sqrt(JumpHeight * -3.0f * gravity);
                animator.SetBool("IsJumping", true);
            }

            velocity.y += gravity * Time.deltaTime;
            controller.Move(velocity * Time.deltaTime);
        }

        private void HandleMovement()
        {
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");

            Vector3 inputDirection = new Vector3(horizontal, 0, vertical);
            Vector3 transformDirection = transform.TransformDirection(inputDirection);

            if (horizontal != 0 || vertical != 0)
            {
                speed = Mathf.Min(speed + forceMagnitude * Time.deltaTime, MaxSpeed);

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