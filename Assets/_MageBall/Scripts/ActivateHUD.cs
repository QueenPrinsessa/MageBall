using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivateHUD : NetworkBehaviour
{

    [SerializeField] private GameObject HUD;

    public override void OnStartAuthority()
    {
        HUD.SetActive(true);
    }


}
