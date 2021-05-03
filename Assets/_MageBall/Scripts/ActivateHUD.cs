using Mirror;
using UnityEngine;

namespace MageBall
{
    public class ActivateHUD : NetworkBehaviour
    {

        [SerializeField] private GameObject HUD;

        public override void OnStartAuthority()
        {
            HUD.SetActive(true);
        }


    }
}