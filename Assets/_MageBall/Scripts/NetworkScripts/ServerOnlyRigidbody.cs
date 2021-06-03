using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

namespace MageBall
{
    /// <summary>
    /// Huvudansvarig: Zoe
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class ServerOnlyRigidbody : NetworkBehaviour
    {
        [SerializeField] private new Rigidbody rigidbody;

        public override void OnStartServer()
        {
            MakeRigidbodyKinematic();
            rigidbody.isKinematic = false;
        }

        [ClientRpc]
        private void MakeRigidbodyKinematic()
        {
            rigidbody.isKinematic = true;
        }
    }
}
