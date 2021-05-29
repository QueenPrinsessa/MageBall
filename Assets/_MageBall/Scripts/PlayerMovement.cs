using Mirror;
using System;
using System.Collections;
using UnityEngine;

namespace MageBall
{
    public class PlayerMovement : NetworkBehaviour
    {

        [Header("Movement Settings")]
        [SerializeField] private float maxSpeed = 10f;
        [SerializeField] private float jumpHeight = 1.3f;
        [SerializeField] private float acceleration = 3.5f;
        [SerializeField] private float deacceleration = 30f;
        [SerializeField] private float dashDistance = 5f;
        [SerializeField] private int dashCooldown = 5;
        [SerializeField] private Passive speedPassive;
        [SerializeField] private Passive jumpPassive;
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip dashSound;
        [SerializeField] private GameObject dashVfx;
        [SerializeField] private float dashVfxDuration = 2f;
        private CharacterController controller;
        private CharacterControllerGravity controllerGravity;
        private Animator animator;
        private Vector3 velocity;
        private float currentSpeed = 0f;
        private float horizontal;
        private float vertical;
        private bool canJump = true;
        private bool canDash = true;
        [SyncVar] private Passives currentPassive;

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

            if (Input.GetButton("Jump") && canJump)
            {
                velocity.y += Mathf.Sqrt(JumpHeight * -3.0f * controllerGravity.Gravity);
                animator.SetBool("IsJumping", true);
                canJump = false;
            }
            else if (isGrounded)
            {
                velocity.y = 0;
                animator.SetBool("IsJumping", false);
                canJump = true;
            }

            controller.Move(velocity * Time.fixedDeltaTime);
        }


        private void HandleMovement()
        {
            horizontal = Input.GetAxis("Horizontal");
            vertical = Input.GetAxis("Vertical");
            animator.SetFloat("Horizontal", horizontal);
            animator.SetFloat("Vertical", vertical);

            Vector3 direction = (transform.right * horizontal + transform.forward * vertical).normalized;
            float dot = Vector3.Dot(transform.forward, direction);

            if ((horizontal != 0f || vertical != 0f))
                currentSpeed = Mathf.Min(currentSpeed + acceleration * Time.fixedDeltaTime, dot < 0.5f ? MaxSpeed/2 : MaxSpeed);
            else
                currentSpeed = Mathf.Max(currentSpeed - deacceleration * Time.fixedDeltaTime, 0f);

            CmdUpdateSpeedOnServer(currentSpeed);

            if (Input.GetButton("Dodge") && canDash)
                StartCoroutine(Dashing(direction));

            animator.SetFloat("Speed", currentSpeed);

            float distance = currentSpeed * Time.fixedDeltaTime;
            controller.Move(distance * direction);
        }

        [Client]
        public void ResetSpeed()
        {
            currentSpeed = 0;
        }

        [Command]
        private void CmdUpdateSpeedOnServer(float speed)
        {
            currentSpeed = speed;
        }

        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            if (!hasAuthority || hit.rigidbody == null)
                return;

            CmdPushRigidbody(hit.gameObject, hit.moveDirection, currentSpeed);
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

        private IEnumerator Dashing(Vector3 direction)
        {
            canDash = false;
            CmdDashOnServer(transform.position);
            controller.Move(direction * dashDistance);

            yield return new WaitForSeconds(dashCooldown);

            canDash = true;
        }

        [Command]
        private void CmdDashOnServer(Vector3 position)
        {
            GameObject vfx = Instantiate(dashVfx, position + Vector3.up, dashVfx.transform.rotation);
            NetworkServer.Spawn(vfx);
            StartCoroutine(DestroyAfterTime(vfx, dashVfxDuration));
            RpcPlayDashSound();
        }

        [ClientRpc]
        private void RpcPlayDashSound()
        {
            audioSource.PlayOneShot(dashSound);
        }

        private IEnumerator DestroyAfterTime(GameObject gameObject, float time)
        {
            yield return new WaitForSeconds(time);

            if(gameObject != null)
                NetworkServer.Destroy(gameObject);
        }

      
    }
}