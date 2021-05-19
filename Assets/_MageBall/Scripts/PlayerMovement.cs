using Mirror;
using System;
using UnityEngine;

namespace MageBall
{
    public class PlayerMovement : NetworkBehaviour
    {
        private CharacterController controller;
        private CharacterControllerGravity controllerGravity;
        private Animator animator;
        [SyncVar] private float speed = 0f;
        [SerializeField] private float maxSpeed = 10f;
        [SerializeField] private float forceMagnitude = 6f;
        [SerializeField] private float jumpHeight = 1.3f;
        [SerializeField] private Passive speedPassive;
        [SerializeField] private Passive jumpPassive;
        [SyncVar] private Passives currentPassive;
        private Vector3 moveDirection;
        private Vector3 velocity;

        private float JumpHeight => currentPassive == Passives.HigherJumping ? jumpHeight * jumpPassive.modifier : jumpHeight;
        private float MaxSpeed => currentPassive == Passives.FasterSpeed ? maxSpeed * speedPassive.modifier : maxSpeed;
        public override void OnStartAuthority()
        {
            controller = GetComponent<CharacterController>();
            controllerGravity = GetComponent<CharacterControllerGravity>();
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

            bool isGrounded = controllerGravity.IsGrounded();

            if (isGrounded)
            {
                moveDirection.y = 0;
                velocity.y = 0;
                animator.SetBool("IsJumping", false);
            }

            if (Input.GetButton("Jump") && isGrounded)
            {
                velocity.y += Mathf.Sqrt(JumpHeight * -3.0f * controllerGravity.Gravity);
                animator.SetBool("IsJumping", true);
            }

            controller.Move(velocity * Time.fixedDeltaTime);
        }

        private void HandleMovement()
        {
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");

            Vector3 inputDirection = new Vector3(horizontal, 0, vertical);
            Vector3 transformDirection = transform.TransformDirection(inputDirection);

            if (horizontal != 0 || vertical != 0)
            {
                CmdSetSpeed(Mathf.Min(speed + forceMagnitude * Time.fixedDeltaTime, MaxSpeed));

                if (vertical < 0)
                    animator.SetBool("RunBackward", true);
                else
                    animator.SetBool("RunBackward", false);
            }
            else
                CmdSetSpeed(0);

            animator.SetFloat("Speed", speed);

            Vector3 flatMovement = speed * Time.fixedDeltaTime * transformDirection;

            moveDirection = new Vector3(flatMovement.x, moveDirection.y, flatMovement.z);
            controller.Move(moveDirection);
        }

        [Client]
        public void ResetSpeed()
        {
            speed = 0;
        }

        [Command]
        private void CmdSetSpeed(float speed)
        {
            if (speed > MaxSpeed)
                this.speed = MaxSpeed;
            else if (speed < 0)
                this.speed = 0;

            this.speed = speed;
        }


        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            if (!hasAuthority || hit.rigidbody == null)
                return;

            CmdPushRigidbody(hit.gameObject, hit.moveDirection, speed);
        }

        [Command]
        private void CmdPushRigidbody(GameObject gameObject, Vector3 moveDirection, float speed)
        {
            Rigidbody rigidbody = gameObject.GetComponent<Rigidbody>();

            if (rigidbody == null || rigidbody.isKinematic)
                return;

            if (moveDirection.y < -0.3)
                return;

            Vector3 pushDirection = new Vector3(moveDirection.x, 0, moveDirection.z);

            rigidbody.velocity = pushDirection * speed * 1.5f;
        }

    }
}