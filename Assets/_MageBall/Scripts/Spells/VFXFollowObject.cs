using Mirror;
using UnityEngine;

namespace MageBall
{
    public class VFXFollowObject : NetworkBehaviour
    {

        [SerializeField] private bool followPosition = true;
        [SerializeField] private bool matchRotation = false;
        [SerializeField] private bool matchScale = false;

        [SyncVar]
        private Transform followTransform;

        public Transform FollowTransform { get => followTransform; set => followTransform = value; }

        void Update()
        {
            if (FollowTransform == null)
            {
                Debug.LogWarning("No follow transform has been set!");
                return;
            }

            transform.position = FollowTransform.position;
            transform.localScale = followTransform.localScale;
        }
    }
}