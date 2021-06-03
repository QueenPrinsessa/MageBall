using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MageBall
{
    /// <summary>
    /// Huvudansvarig: Hammi
    /// </summary>
    public class Billboard : MonoBehaviour
    {
        private Transform mainCamera;

        private void Start()
        {
            mainCamera = Camera.main.transform;
        }

        void LateUpdate()
        {
            transform.LookAt(transform.position + mainCamera.transform.forward);
        }
    }
}