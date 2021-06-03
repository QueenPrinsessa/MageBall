using Mirror;
using UnityEngine;

namespace MageBall
{
    /// <summary>
    /// Huvudansvarig: Zoe
    /// </summary>
    public class ActivateHUD : NetworkBehaviour
    {

        [SerializeField] private GameObject HUD;

        public override void OnStartAuthority()
        {
            HUD.SetActive(true);
        }

    }
}