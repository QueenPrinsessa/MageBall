using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using TMPro;

public class PlayerNameTag : NetworkBehaviour
{
    [SyncVar(hook = nameof(OnNameChange))] string playerName;
    public TMP_Text nameTag;

    void OnNameChange(string oldPlayerName, string playerName)
    {              
       RpcSetPlayerName(playerName);
    }

    [ClientRpc]
    void RpcSetPlayerName(string name)
    {
        nameTag.text = playerName;
    }

    [Server]
    public void SetPlayerName(string name)
    {
        playerName = name;
    }
}
