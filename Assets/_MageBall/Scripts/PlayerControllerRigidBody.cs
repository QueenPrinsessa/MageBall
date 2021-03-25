using Cinemachine;
using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControllerRigidBody : NetworkBehaviour
{
    [SerializeField] private Transform cameraFollow;
    [SerializeField] private GameObject thirdPersonCamPrefab;
    private CinemachineVirtualCamera thirdPersonCam;
    private new Rigidbody rigidbody;
    private float speed = 600f;
    private float jumpForce = 100f;
    private float mouseSensitivity = 0.5f;
    private float cameraSpeed = 300f;
    private float xRotation = 0f;
    private float groundCheckDistance = 0.25f;
    private Vector2 moveAxis;
    private Vector2 lookAxis;
    private bool isGrounded;

    // Test Test

    public override void OnStartLocalPlayer()
    {
        thirdPersonCam = Instantiate(thirdPersonCamPrefab).GetComponent<CinemachineVirtualCamera>();
        thirdPersonCam.Follow = cameraFollow;
        Cursor.lockState = CursorLockMode.Locked;
        thirdPersonCam.enabled = true;
        rigidbody = GetComponent<Rigidbody>();
    }

    [ClientCallback]
    private void Update()
    {
        if (!isLocalPlayer)
            return;

        if (Input.GetKeyDown(KeyCode.F1))
            Cursor.lockState = CursorLockMode.None;
        else if(Input.GetKeyDown(KeyCode.F2))
            Cursor.lockState = CursorLockMode.Locked;
    }



    //Client authority test:
    [Client]
    private void FixedUpdate()
    {
        if (!isLocalPlayer)
            return;

        if (Physics.Raycast(new Ray(transform.position, -transform.up), out RaycastHit hit, groundCheckDistance))
            isGrounded = true;

        lookAxis = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));
        moveAxis = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        Quaternion rot = transform.rotation * Quaternion.AngleAxis(lookAxis.x * cameraSpeed * mouseSensitivity * Time.fixedDeltaTime, Vector3.up);

        Vector3 direction = (transform.right * moveAxis.x + transform.forward * moveAxis.y).normalized;
        Vector3 vel = direction * speed * Time.fixedDeltaTime;

        float xRotAngle = lookAxis.y * cameraSpeed * mouseSensitivity * Time.fixedDeltaTime;
        xRotation -= xRotAngle;
        xRotation = Mathf.Clamp(xRotation, -80f, 80f);
        cameraFollow.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        MovePlayer(rot, vel, Input.GetButton("Jump") && isGrounded);
    }

    [Client]
    private void MovePlayer(Quaternion rot, Vector3 vel, bool jump)
    {
        rigidbody.MoveRotation(rot);

        vel.y = rigidbody.velocity.y;
        rigidbody.velocity = vel;

        if (jump)
        {
            rigidbody.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            isGrounded = false;
        }
    }


    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Ray groundCheckRay = new Ray(transform.position, -transform.up);
        Gizmos.DrawLine(groundCheckRay.origin, (groundCheckRay.direction.normalized * groundCheckDistance) + groundCheckRay.origin);
    }
}
