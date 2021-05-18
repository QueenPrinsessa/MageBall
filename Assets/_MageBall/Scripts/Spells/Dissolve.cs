using Mirror;
using System.Collections;
using UnityEngine;

namespace QueenPrinsessa
{
    /// <summary>
    /// This script and the shader it uses is adapted from one of my older project
    /// -QueenPrinsessa
    /// </summary>
    public class Dissolve : NetworkBehaviour
    {
        [Header("Dissolve Settings")]
        [SerializeField] private float timeUntilDissolveInSeconds = 60;
        [SerializeField] private float dissolveSpeed = 1f;
        [Range(-1, 1), SerializeField] private float startDissolve = -1;

        private Renderer[] renderers;
        private float currentDissolveAmount;
        private readonly string dissolve = "_Dissolve";

        /// <summary>
        /// -1 (fully solid) is 0 and 1 (dissolved) is done
        /// </summary>
        private float Progress { get; set; } = 0;


        private void Start()
        {
            currentDissolveAmount = startDissolve;
            renderers = GetComponentsInChildren<Renderer>();
            for (int i = 0; i < renderers.Length; i++)
            {
                for (int j = 0; j < renderers[i].materials.Length; j++)
                {
                    renderers[i].materials[j].SetFloat(dissolve, currentDissolveAmount);
                }
            }
        }

        public override void OnStartServer()
        {
            StartCoroutine(DissolveObject());
        }

        [Server]
        private IEnumerator DissolveObject()
        {
            yield return new WaitForSeconds(timeUntilDissolveInSeconds);

            bool hasDissolved = false;

            while (!hasDissolved)
            {
                hasDissolved = Progress == 1;
                RpcDissolveObject();
                yield return null;
            }

            NetworkServer.Destroy(gameObject);
        }

        [ClientRpc]
        private void RpcDissolveObject()
        {
            IncreaseDissolveAmount();
        }

        private void IncreaseDissolveAmount()
        {
            float newDissolve = currentDissolveAmount += dissolveSpeed  * Time.deltaTime;

            for (int i = 0; i < renderers.Length; i++)
            {
                for (int j = 0; j < renderers[i].materials.Length; j++)
                {
                    renderers[i].materials[j].SetFloat(dissolve, newDissolve);
                }
            }

            float progress = (newDissolve + 1) / 2;
            progress = Mathf.Clamp(progress, 0, 1);
            Progress = progress;
        }
    }
}