using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

namespace MageBall
{
    public class ThirdPersonCamera : NetworkBehaviour
    {


        private Transform cameraFollow;
        private CinemachineVirtualCamera thirdPersonCam; // the main vcam that we're using
        [SerializeField] private GameObject thirdPersonCamPrefab;
        private float verticalRotateMin = -80f;
        private float verticalRotateMax = 80f;

        //Camera Movement Multiplier
        private float cameraVerticalRotationMultiplier = 2f;
        private float cameraHorizontalRotationMultiplier = 2f;

        //Camera Input Values
        public float cameraInputHorizontal;
        public float cameraInputVertical;

        [Header("Invert Camera Controls")]
        public bool invertHorizontal = false;
        public bool invertVertical = false;

        [Header("Toggles which side the camera should start on. 1 = Right, 0 = Left")]
        public float cameraSide = 1f;

        [Header("Allow toggling left to right shoulder")]
        public bool allowCameraToggle = true;

        [Header("How fast we should transition from left to right")]
        public float cameraSideToggleSpeed = 1f;

        private Cinemachine3rdPersonFollow followCam; // so we can manipulate the 'camera side' property dynamically

        // current camera rotation values
        private float cameraX = 0f;
        private float cameraY = 0f;

        // if we are switching sides
        private bool doCameraSideToggle = false;
        private float sideToggleTime = 0f;
        // where we are in the transition from side to side
        private float desiredCameraSide = 1f;

        private void start()
        {
            if (thirdPersonCam == null)
            {
                // try to grab the thirdPersonCam from this object
                thirdPersonCam = GetComponent<CinemachineVirtualCamera>();
            }
            else
            {
                Debug.Log("Need to connect your 3rd person thirdPersonCam to the CameraController!");
            }
        }

        //public override void OnStartAuthority() // how the fk does mirror work
        //{
        //    if(thirdPersonCam == null)
        //    {   
        //        thirdPersonCam = Instantiate(thirdPersonCamPrefab).GetComponent<CinemachineVirtualCamera>();
        //    }
        //    else
        //    {
        //        Debug.Log("Need to connect your 3rd person thirdPersonCam to the CameraController!");
        //    }

        //    thirdPersonCam.Follow = cameraFollow;
        //    Cursor.lockState = CursorLockMode.Locked;
        //    thirdPersonCam.enabled = true;
        //}



        private void Update()
        {
            // make sure we have a handle to the follow component
            if (followCam == null)
            {
                followCam = thirdPersonCam.GetCinemachineComponent<Cinemachine3rdPersonFollow>();
            }

            cameraInputHorizontal = -Input.GetAxis("Mouse X");
            cameraInputVertical = Input.GetAxis("Mouse Y");
       


            if (cameraFollow != null)
            {
                if (invertHorizontal)
                {
                cameraX -= cameraVerticalRotationMultiplier * cameraInputVertical;
                }
                else
                {
                cameraX += cameraVerticalRotationMultiplier * cameraInputVertical;
                }

                if (invertVertical)
                {
                cameraY -= cameraHorizontalRotationMultiplier * cameraInputHorizontal;
                }
                else
                {
                cameraY += cameraHorizontalRotationMultiplier * cameraInputHorizontal;
                }

                cameraFollow.eulerAngles = new Vector3(cameraX, cameraY, 0.0f);
            }
        }




    }
}

