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
        nameTag.text = playerName;
    }

    [ClientRpc]
    void SetPlayerName(string oldName, string name)
    {
        transform.name = name;
        gameObject.transform.Find("PlayerName_canvas").transform.Find("nameTag").GetComponent<TextMeshProUGUI>().text = name;
    }
}
