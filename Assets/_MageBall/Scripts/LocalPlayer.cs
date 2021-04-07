using Cinemachine;
using Mirror;
using System.Collections.Generic;
using UnityEngine;

namespace MageBall
{
    public class LocalPlayer : MonoBehaviour
    {
        [SerializeField] private LayerMask groundLayerMask;
        [SerializeField] private Transform cameraFollow;
        [SerializeField] private GameObject thirdPersonCamPrefab;
        private CinemachineVirtualCamera thirdPersonCam;
        private CharacterController controller;
        [SerializeField] private float speed = 8f;
        [SerializeField] private float jumpHeight = 0.9f;
        [SerializeField] private float gravity = -20.0f;
        //private float gravityTime;
        //private bool gravityForce;
        private float mouseSensitivity = 0.5f;
        private float cameraSpeed = 300f;
        private float xRotation = 0f;
        private float groundCheckDistance = 0.25f;
        private Vector2 moveAxis;
        private Vector2 lookAxis;
        private Vector3 velocity;
        private bool isGrounded;

        public void Start()
        {
            thirdPersonCam = Instantiate(thirdPersonCamPrefab).GetComponent<CinemachineVirtualCamera>();
            thirdPersonCam.Follow = cameraFollow;
            Cursor.lockState = CursorLockMode.Locked;
            thirdPersonCam.enabled = true;
            controller = GetComponent<CharacterController>();
        }

        private void Update()
        {

            if (Input.GetKeyDown(KeyCode.F1))
                Cursor.lockState = CursorLockMode.None;
            else if (Input.GetKeyDown(KeyCode.F2))
                Cursor.lockState = CursorLockMode.Locked;
        }


        private void FixedUpdate()
        {

            float xRotAngle = lookAxis.y * cameraSpeed * mouseSensitivity * Time.fixedDeltaTime;
            xRotation -= xRotAngle;
            xRotation = Mathf.Clamp(xRotation, -80f, 80f);
            cameraFollow.localRotation = Quaternion.Euler(xRotation, 0f, 0f);


            lookAxis = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));
            moveAxis = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

            Quaternion rotation = transform.rotation * Quaternion.AngleAxis(lookAxis.x * cameraSpeed * mouseSensitivity * Time.fixedDeltaTime, Vector3.up);
            transform.rotation = rotation;

            if (IsGrounded() && this.velocity.y < 0)
                this.velocity.y = 0;

            Vector3 direction = (transform.right * moveAxis.x + transform.forward * moveAxis.y).normalized;
            Vector3 velocity = direction * speed * Time.fixedDeltaTime;
            controller.Move(velocity);

            if (Input.GetButton("Jump") && IsGrounded())
                this.velocity.y += Mathf.Sqrt(jumpHeight * -3.0f * gravity);

            this.velocity.y += gravity * Time.fixedDeltaTime * 2;
            controller.Move(this.velocity * Time.fixedDeltaTime);
            
        }
        //Physics.Raycast(new Ray(transform.position, -transform.up), out RaycastHit hit, groundCheckDistance)
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
            Debug.Log(raycastHit.collider);
            return raycastHit.collider != null;
        }

        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            if (hit.rigidbody == null || hit.rigidbody.isKinematic)
                return;

            if (hit.moveDirection.y < -0.3)
                return;

            Vector3 pushDirection = new Vector3(hit.moveDirection.x, 0, hit.moveDirection.z);

            CmdPushRigidbody(hit.gameObject, pushDirection * speed * 1.5f);
        }

        private void CmdPushRigidbody(GameObject go, Vector3 push) => go.GetComponent<Rigidbody>().velocity = push;


        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Ray groundCheckRay = new Ray(transform.position, -transform.up);
            Gizmos.DrawLine(groundCheckRay.origin, (groundCheckRay.direction.normalized * groundCheckDistance) + groundCheckRay.origin);
        }

    }
}