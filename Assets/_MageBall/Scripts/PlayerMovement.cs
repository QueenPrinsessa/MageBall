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
        [SerializeField] private float jumpHeight = 0.9f;
        [SerializeField] private float gravity = -10.0f;
        [SerializeField] private Passive speedPassive;
        private float groundCheckDistance = 0.25f;
        private Vector3 moveDirection;
        private Vector3 velocity;

        public override void OnStartAuthority()
        {
            controller = GetComponent<CharacterController>();
            animator = GetComponent<Animator>();
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
                speed = Mathf.Min(speed + forceMagnitude * Time.deltaTime, maxSpeed * speedPassive.modifier);
            }
            else
            {
                speed = 0f;
                //speed = Mathf.Max(speed - forceMagnitude * Time.deltaTime * 1.5f, 0);
            }

            RunBackward();

            Dance();

            animator.SetFloat("Speed", speed);

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
            {
                velocity.y += Mathf.Sqrt(jumpHeight * -3.0f * gravity);
                StartCoroutine(Jump());
            }   

            velocity.y += gravity * Time.deltaTime;
            controller.Move(velocity * Time.deltaTime);
        }
        

        private bool IsGrounded()
        {
            Physics.Raycast(new Ray(transform.position, -transform.up), out RaycastHit raycastHit, groundCheckDistance);
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

        private IEnumerator Jump()
        {
            animator.SetBool("IsJumping", true);
    
            yield return new WaitForSeconds(1);

            animator.SetBool("IsJumping", false);
        }

        private void Dance()
        {
            if (Input.GetKey("j"))
            {
                animator.SetBool("Dance1", true);
            }
            if (!Input.GetKey("j"))
            {
                animator.SetBool("Dance1", false);
            }

            if (Input.GetKey("k"))
            {
                animator.SetBool("Dance2", true);
            }
            if (!Input.GetKey("k"))
            {
                animator.SetBool("Dance2", false);
            }

            if (Input.GetKey("l"))
            {
                animator.SetBool("Dance3", true);
            }
            if (!Input.GetKey("l"))
            {
                animator.SetBool("Dance3", false);
            }
        }

        private void RunBackward()
        {
            if (Input.GetKey("s"))
            {
                animator.SetBool("RunBackward", true);
            }
            if (!Input.GetKey("s"))
            {
                animator.SetBool("RunBackward", false);
            }
        }
    }
}