using Cinemachine;
using Mirror;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControllerCharacterController : NetworkBehaviour
{
    [SerializeField] private Transform cameraFollow;
    [SerializeField] private GameObject thirdPersonCamPrefab;
    private CinemachineVirtualCamera thirdPersonCam;
    private CharacterController controller;
    private float speed = 8f;
    private float jumpHeight = 0.9f;
    private float mouseSensitivity = 0.5f;
    private float cameraSpeed = 300f;
    private float xRotation = 0f;
    private float groundCheckDistance = 0.25f;
    private float gravity = -20f;
    private Vector2 moveAxis;
    private Vector2 lookAxis;
    private Vector3 playerVelocity;
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

        if (Physics.Raycast(new Ray(transform.position, -transform.up), out RaycastHit hit, groundCheckDistance)) //switch to sphere cast for better results
            isGrounded = true;
        else
            isGrounded = false;

        lookAxis = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));
        moveAxis = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        Quaternion rot = transform.rotation * Quaternion.AngleAxis(lookAxis.x * cameraSpeed * mouseSensitivity * Time.fixedDeltaTime, Vector3.up);
        transform.rotation = rot;

        if (isGrounded && playerVelocity.y < 0)
            playerVelocity.y = 0;

        Vector3 direction = (transform.right * moveAxis.x + transform.forward * moveAxis.y).normalized;
        Vector3 velocity = direction * speed * Time.fixedDeltaTime;
        controller.Move(velocity);

        if (Input.GetButton("Jump") && isGrounded)
            playerVelocity.y += Mathf.Sqrt(jumpHeight * -3.0f * gravity);

        playerVelocity.y += gravity * Time.fixedDeltaTime;
        controller.Move(playerVelocity * Time.fixedDeltaTime);
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