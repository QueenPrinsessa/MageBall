using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MageBall
{
    public abstract class Spell : NetworkBehaviour
    {
        private Transform cameraPosition;
        private Transform aimPoint;

        [SyncVar] protected Vector3 aimPosition;
        [SyncVar] protected Vector3 aimForward;
        [SyncVar] protected Quaternion aimRotation;

        [Client]
        private void Update()
        {
            if (!hasAuthority)
                return;

            if (cameraPosition == null)
            {
                ThirdPersonCamera camera = GetComponent<ThirdPersonCamera>();
                if (camera != null)
                {
                    cameraPosition = camera.ThirdPersonVirtualCamera.transform;
                    aimPoint = camera.ThirdPersonVirtualCamera.Follow;
                }
                else
                {
                    Debug.LogError("No third person camera script on player!");
                    return;
                }
            }

            CmdSetAim(cameraPosition.position, aimPoint.forward, aimPoint.rotation);
        }

        [Command]
        private void CmdSetAim(Vector3 aimPosition, Vector3 aimForward, Quaternion aimRotation)
        {
            this.aimPosition = aimPosition;
            this.aimForward = aimForward;
            this.aimRotation = aimRotation;
        }

        [Command]
        public virtual void CmdCastSpell() { }

    }
}