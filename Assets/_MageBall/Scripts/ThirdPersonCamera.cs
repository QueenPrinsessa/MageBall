using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;

namespace MageBall
{
    public class ThirdPersonCamera : NetworkBehaviour
    {

        [SerializeField] private Transform cameraFollow;
        [SerializeField] private GameObject thirdPersonCameraPrefab;

        private CinemachineVirtualCamera thirdPersonVirtualCamera;

        private float cameraVerticalRotationMultiplier = 100f;
        private float cameraHorizontalRotationMultiplier = 100f;
        private float mouseSensitivity = 1;

        private bool invertMouseY = false;
        private bool invertMouseX = false;

        [Header("X Rotation Clamping")]
        [SerializeField, Range(-90f, 90f)] private float minXRotation = -80f;
        [SerializeField, Range(-90f, 90f)] private float maxXRotation = 80f;

        [Header("Toggles which side the camera should start on. 1 = Right, 0 = Left")]
        [SerializeField] private float cameraSide = 1f;

        [Header("Allow toggling left to right shoulder")]
        [SerializeField] private bool allowCameraToggle = true;

        [Header("How fast we should transition from left to right")]
        [SerializeField] private float cameraSideToggleSpeed = 1f;

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
            LoadCameraControlSettings();

            OptionsMenu.ControlSettingsChanged += OnControlSettingsChanged;

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

        private void OnDestroy()
        {
            OptionsMenu.ControlSettingsChanged -= OnControlSettingsChanged;
        }

        [Client]
        private void OnControlSettingsChanged()
        {
            LoadCameraControlSettings();
        }

        [Client]
        private void LoadCameraControlSettings()
        {
            mouseSensitivity = PlayerPrefs.GetFloat(OptionsMenu.MouseSensitivityPlayerPrefsKey, 1f);
            invertMouseY = Convert.ToBoolean(PlayerPrefs.GetInt(OptionsMenu.InvertMouseYAxisPlayerPrefsKey, Convert.ToInt32(false)));
            invertMouseX = Convert.ToBoolean(PlayerPrefs.GetInt(OptionsMenu.InvertMouseXAxisPlayerPrefsKey, Convert.ToInt32(false)));
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

            float cameraInputHorizontal = Input.GetAxis("Mouse X");
            float cameraInputVertical = Input.GetAxis("Mouse Y");

            if (invertMouseY)
            {
                cameraX += cameraVerticalRotationMultiplier * cameraInputVertical * mouseSensitivity * Time.fixedDeltaTime;
            }
            else
            {
                cameraX -= cameraVerticalRotationMultiplier * cameraInputVertical * mouseSensitivity * Time.fixedDeltaTime;
            }

            //Clamp X rotation
            cameraX = Mathf.Clamp(cameraX, minXRotation, maxXRotation);

            //Use local angles
            cameraFollow.localEulerAngles = new Vector3(cameraX, 0.0f, 0.0f);

            //Rotate player for Y, not camera
            int mouseXInvertionFactor = invertMouseX ? -1 : 1;
            transform.rotation *= Quaternion.AngleAxis(mouseXInvertionFactor * cameraInputHorizontal * mouseSensitivity * cameraHorizontalRotationMultiplier * Time.fixedDeltaTime, Vector3.up);
        }




    }
}

