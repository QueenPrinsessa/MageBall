using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

namespace MageBall
{
    public class ThirdPersonCamera : NetworkBehaviour
    {

        [SerializeField] private Transform cameraFollow;
        [SerializeField] private GameObject thirdPersonCameraPrefab;

        private CinemachineVirtualCamera thirdPersonVirtualCamera;

        //Camera Movement Multiplier
        private float cameraVerticalRotationMultiplier = 100f;
        private float cameraHorizontalRotationMultiplier = 100f;

        //Camera Input Values
        public float cameraInputHorizontal;
        public float cameraInputVertical;

        //Inverting vertical and horizontal is usually called invert X and invert Y since they refer to inverting the mouse direction
        [Header("Invert Camera Controls")]
        public bool invertMouseY = false;
        public bool invertMouseX = false;

        [Header("X Rotation Clamping")]
        [SerializeField, Range(-90f, 90f)] private float minXRotation = -80f;
        [SerializeField, Range(-90f, 90f)] private float maxXRotation = 80f;

        [Header("Toggles which side the camera should start on. 1 = Right, 0 = Left")]
        public float cameraSide = 1f;

        [Header("Allow toggling left to right shoulder")]
        public bool allowCameraToggle = true;

        [Header("How fast we should transition from left to right")]
        public float cameraSideToggleSpeed = 1f;

        private Cinemachine3rdPersonFollow followCam; // so we can manipulate the 'camera side' property dynamically

        // current camera rotation values
        private float cameraX = 0f;

        // if we are switching sides
        private bool doCameraSideToggle = false;
        private float sideToggleTime = 0f;

        // where we are in the transition from side to side
        private float desiredCameraSide = 1f;

        public CinemachineVirtualCamera ThirdPersonVirtualCamera => thirdPersonVirtualCamera;

        public override void OnStartAuthority()
        {
            GameObject thirdPersonCamera = Instantiate(thirdPersonCameraPrefab);

            if (thirdPersonVirtualCamera == null)
            {
                thirdPersonVirtualCamera = thirdPersonCamera.GetComponent<CinemachineVirtualCamera>();
                thirdPersonVirtualCamera.Follow = cameraFollow;
                thirdPersonVirtualCamera.enabled = true;
                followCam = thirdPersonVirtualCamera.GetCinemachineComponent<Cinemachine3rdPersonFollow>();
            }
            else
            {
                Debug.LogError("Need to connect your 3rd person camera to the CameraController!");
            }

            Cursor.lockState = CursorLockMode.Locked; 
        }

        private void FixedUpdate()
        {
            if (!hasAuthority)
                return;

            //Since everything that follows depends on the Camera Follow existing
            //A guard statement is more appropriate than wrapping everything in an if statement
            if (cameraFollow == null)
            {
                Debug.LogError("Camera follow not assigned in script.");
                return;
            }

            cameraInputHorizontal = Input.GetAxis("Mouse X");
            cameraInputVertical = Input.GetAxis("Mouse Y");

            if (invertMouseY)
            {
                cameraX += cameraVerticalRotationMultiplier * cameraInputVertical * Time.fixedDeltaTime;
            }
            else
            {
                cameraX -= cameraVerticalRotationMultiplier * cameraInputVertical * Time.fixedDeltaTime;
            }

            //Clamp X rotation
            cameraX = Mathf.Clamp(cameraX, minXRotation, maxXRotation);

            //Use local angles
            cameraFollow.localEulerAngles = new Vector3(cameraX, 0.0f, 0.0f);

            //Rotate player for Y, not camera
            int mouseXInvertionFactor = invertMouseX ? -1 : 1;
            transform.rotation *= Quaternion.AngleAxis(mouseXInvertionFactor * cameraInputHorizontal * cameraHorizontalRotationMultiplier * Time.fixedDeltaTime, Vector3.up);
        }




    }
}

