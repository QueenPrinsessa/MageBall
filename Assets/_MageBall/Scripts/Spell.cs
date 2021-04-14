using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MageBall
{
    public abstract class Spell : NetworkBehaviour
    {
        protected Transform aimPoint;

        private void Start()
        {
            aimPoint = GetComponent<ThirdPersonCamera>().ThirdPersonVirtualCamera.transform;
        }

        [Command]
        public virtual void CmdCastSpell(Vector3 position, Quaternion rotation) { }

    }
}