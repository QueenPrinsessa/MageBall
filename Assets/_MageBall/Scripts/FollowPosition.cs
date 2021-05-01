using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowPosition : NetworkBehaviour
{

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
    }
}
