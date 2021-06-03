using Mirror;
using System.Collections;
using UnityEngine;

namespace MageBall
{
    public abstract class Spell : NetworkBehaviour
    {
        [Header("Spell settings")]
        [SerializeField] private float manaCost = 30f;
        [SerializeField, Tooltip("Positive numbers moves aim closer to camera. Negative numbers moves aim further away from the camera")]
        private float aimOffset = 0f;
        private Transform cameraPosition;
        private Transform aimPoint;
        private Animator animator;

        [SyncVar] protected Vector3 aimPosition;
        [SyncVar] protected Vector3 aimForward;
        [SyncVar] protected Quaternion aimRotation;

        public float ManaCost => manaCost;

        public override void OnStartAuthority()
        {
            animator = GetComponent<Animator>();

            if (animator == null)
                Debug.LogError("Spell script can't find the animator. Is the script attached to the player gameobject?");
            
            StartCoroutine(FindThirdPersonCamera());
        }

        [Client]
        private IEnumerator FindThirdPersonCamera()
        {
            ThirdPersonCamera camera = GetComponent<ThirdPersonCamera>();

            yield return new WaitUntil(() => camera.ThirdPersonVirtualCamera != null);

            cameraPosition = camera.ThirdPersonVirtualCamera.transform;
            aimPoint = camera.ThirdPersonVirtualCamera.Follow;
        }

        [ClientCallback]
        private void Update()
        {
            if (!hasAuthority || cameraPosition == null)
                return;

            CmdSetAim(cameraPosition.position + aimPoint.forward.normalized * ((transform.position - cameraPosition.position).magnitude - aimOffset), aimPoint.forward, aimPoint.rotation);
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

        [TargetRpc]
        protected void TargetTriggerAttackAnimation(string attackTrigger)
        {
            animator.SetTrigger(attackTrigger);
        }

        [Server]
        protected IEnumerator DestroyVFX(GameObject vfx, float seconds)
        {
            yield return new WaitForSeconds(seconds);

            if (vfx != null)
                NetworkServer.Destroy(vfx);
        }

    }
}