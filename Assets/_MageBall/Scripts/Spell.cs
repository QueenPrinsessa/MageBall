using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Spell : NetworkBehaviour
{
    protected Transform castPoint;

    public override void OnStartServer()
    {
        castPoint = Camera.main.transform;
    }

    [Command]
    public virtual void CmdCastSpell() { }

}
