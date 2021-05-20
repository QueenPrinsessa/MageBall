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

        [Header("Camera Settings")]
        [SerializeField] private Transform cameraFollow;
        [SerializeField] private GameObject thirdPersonCameraPrefab;
        [SerializeField] private float cameraVerticalRotationMultiplier = 100f;
        [SerializeField] private float cameraHorizontalRotationMultiplier = 100f;

        [Header("X Rotation Clamping")]
        [SerializeField, Range(-90f, 90f)] private float minXRotation = -80f;
        [SerializeField, Range(-90f, 90f)] private float maxXRotation = 80f;

        private CinemachineVirtualCamera thirdPersonVirtualCamera;
        private float lookSensitivity = 1;
        private float cameraXRotation = 0f;
        private bool invertY = false;
        private bool invertX = false;

        public CinemachineVirtualCamera ThirdPersonVirtualCamera => thirdPersonVirtualCamera;

        public override void OnStartAuthority()
        {
            LoadCameraControlSettings();

            GameObject thirdPersonCamera = Instantiate(thirdPersonCameraPrefab);
            thirdPersonVirtualCamera = thirdPersonCamera.GetComponent<CinemachineVirtualCamera>();
            thirdPersonVirtualCamera.Follow = cameraFollow;
            thirdPersonVirtualCamera.enabled = true;

            Cursor.lockState = CursorLockMode.Locked;

            OptionsMenu.ControlSettingsChanged += OnControlSettingsChanged;
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
            lookSensitivity = PlayerPrefs.GetFloat(OptionsMenu.MouseSensitivityPlayerPrefsKey, 1f);
            invertY = Convert.ToBoolean(PlayerPrefs.GetInt(OptionsMenu.InvertMouseYAxisPlayerPrefsKey, Convert.ToInt32(false)));
            invertX = Convert.ToBoolean(PlayerPrefs.GetInt(OptionsMenu.InvertMouseXAxisPlayerPrefsKey, Convert.ToInt32(false)));
        }

        [ClientCallback]
        private void FixedUpdate()
        {
            if (!hasAuthority)
                return;

            float cameraInputHorizontal = Input.GetAxis("Mouse X");
            float cameraInputVertical = Input.GetAxis("Mouse Y");

            if (invertY)
                cameraXRotation += cameraVerticalRotationMultiplier * cameraInputVertical * lookSensitivity * Time.fixedDeltaTime;
            else
                cameraXRotation -= cameraVerticalRotationMultiplier * cameraInputVertical * lookSensitivity * Time.fixedDeltaTime;

            cameraXRotation = Mathf.Clamp(cameraXRotation, minXRotation, maxXRotation);
            cameraFollow.localEulerAngles = new Vector3(cameraXRotation, 0.0f, 0.0f);

            int xInvertionFactor = invertX ? -1 : 1;
            transform.rotation *= Quaternion.AngleAxis(xInvertionFactor * cameraInputHorizontal * lookSensitivity * cameraHorizontalRotationMultiplier * Time.fixedDeltaTime, Vector3.up);
        }




    }
}

