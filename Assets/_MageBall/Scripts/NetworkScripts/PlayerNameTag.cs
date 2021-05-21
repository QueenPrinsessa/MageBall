using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using TMPro;

public class PlayerNameTag : NetworkBehaviour
{
    [SyncVar(hook ="OnNameChange")] string playerName;
    public TMP_Text nameTag;

    void OnNameChange(string playerName)
    {
        nameTag.text = playerName;
    }


}
