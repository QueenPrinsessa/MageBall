using Cinemachine;
using Mirror;
using System.Collections.Generic;
using UnityEngine;

namespace MageBall
{
    public class Player : NetworkBehaviour
    {
        [SerializeField] private LayerMask groundLayerMask;
        [SerializeField] private Transform cameraFollow;
        [SerializeField] private GameObject thirdPersonCamPrefab;
        private CinemachineVirtualCamera thirdPersonCam;
        private CharacterController controller;
        private float speed = 0f;
        [SerializeField] private float maxSpeed = 10f;
        [SerializeField] private float forceMagnitude = 4f; 
        [SerializeField] private float jumpHeight = 0.9f;
        [SerializeField] private float gravity = -10.0f;
        private float mouseSensitivity = 0.5f;
        private float cameraSpeed = 300f;
        private float xRotation = 0f;
        private float groundCheckDistance = 0.25f;
        private Vector3 moveDirection;
        private Vector2 lookAxis;
        private Vector3 velocity;
        private bool isGrounded;

        public override void OnStartLocalPlayer()
        {
            thirdPersonCam = Instantiate(thirdPersonCamPrefab).GetComponent<CinemachineVirtualCamera>();
            thirdPersonCam.Follow = cameraFollow;
            Cursor.lockState = CursorLockMode.Locked;
            thirdPersonCam.enabled = true;
            controller = GetComponent<CharacterController>();
        }

        [ClientCallback]
        private void Update()
        {
            if (!isLocalPlayer)
                return;

            if (Input.GetKeyDown(KeyCode.F1))
                Cursor.lockState = CursorLockMode.None;
            else if (Input.GetKeyDown(KeyCode.F2))
                Cursor.lockState = CursorLockMode.Locked;
        }

        [Client]
        private void FixedUpdate()
        {
            if (!isLocalPlayer)
                return;

            float xRotAngle = lookAxis.y * cameraSpeed * mouseSensitivity * Time.fixedDeltaTime;
            xRotation -= xRotAngle;
            xRotation = Mathf.Clamp(xRotation, -80f, 80f);

            cameraFollow.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

            lookAxis = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));

            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");
            Quaternion rotation = transform.rotation * Quaternion.AngleAxis(lookAxis.x * cameraSpeed * mouseSensitivity * Time.fixedDeltaTime, Vector3.up);
            transform.rotation = rotation;

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

            Debug.Log(speed);
           
            Vector3 flatMovement = speed * Time.deltaTime * transformDirection;

            moveDirection = new Vector3(flatMovement.x, moveDirection.y, flatMovement.z);
            controller.Move(moveDirection);

            if (IsGrounded())
            {
                moveDirection.y = 0;
                velocity.y = 0;
            }

            

            if (Input.GetButton("Jump") && IsGrounded())
                this.velocity.y += Mathf.Sqrt(jumpHeight * -3.0f * gravity);

            this.velocity.y += gravity * Time.deltaTime;
            controller.Move(this.velocity * Time.deltaTime);

        }

        private bool IsGrounded()
        {

            Physics.Raycast(new Ray(transform.position, -transform.up), out RaycastHit raycastHit, groundCheckDistance, groundLayerMask); //switch to sphere cast for better results
            Color rayColor;
            if(raycastHit.collider != null) // if the raycast is not null it has hit something
            {
                rayColor = Color.green; // green for grounded
            }
            else
            {
                rayColor = Color.red; // red for floating?
            }
 
            Debug.DrawRay(transform.position, -transform.up * groundCheckDistance);
            return raycastHit.collider != null;
        }

        [Client]
        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            if (hit.rigidbody == null || hit.rigidbody.isKinematic)
                return;

            if (hit.moveDirection.y < -0.3)
                return;

            Vector3 pushDirection = new Vector3(hit.moveDirection.x, 0, hit.moveDirection.z);

            CmdPushRigidbody(hit.gameObject, pushDirection * speed * 1.5f);
        }

        [Command]
        private void CmdPushRigidbody(GameObject go, Vector3 push) => go.GetComponent<Rigidbody>().velocity = push;


        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Ray groundCheckRay = new Ray(transform.position, -transform.up);
            Gizmos.DrawLine(groundCheckRay.origin, (groundCheckRay.direction.normalized * groundCheckDistance) + groundCheckRay.origin);
        }

    }
}