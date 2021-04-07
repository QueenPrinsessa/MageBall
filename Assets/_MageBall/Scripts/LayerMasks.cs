using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class LayerMasks
{

    public static readonly LayerMask ballLayer;
    public static readonly LayerMask playerLayer;
    public static readonly LayerMask groundLayer;

    static LayerMasks()
    {
        ballLayer = LayerMask.GetMask("Ball");
        playerLayer = LayerMask.GetMask("Player");
        groundLayer = LayerMask.GetMask("Ground");
    }
}
