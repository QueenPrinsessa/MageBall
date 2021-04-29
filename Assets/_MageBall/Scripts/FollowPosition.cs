using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowPosition : NetworkBehaviour
{

    public Transform FollowTransform { get; set; }
    
    [Server]
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
