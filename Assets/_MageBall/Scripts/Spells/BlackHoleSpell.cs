using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

namespace MageBall
{
    public class BlackHoleSpell : LineSpell
    {
        [Header("Black Hole Settings")]
        [SerializeField] private GameObject blackHolePrefab;
        [SerializeField] private Vector3 blackHoleOffset = new Vector3(0, 2, 0);   

        [Command]
        public override void CmdCastSpell()
        {
            TargetTriggerAttackAnimation("Attack1");
            if (Physics.Raycast(aimPosition, aimForward, out RaycastHit hit, Range, LayerMasks.ballLayer | LayerMasks.propsLayer | LayerMasks.groundLayer))
            {
                Vector3 spawnPosition = hit.point + blackHoleOffset;
                GameObject blackHole = Instantiate(blackHolePrefab, spawnPosition, aimRotation);
                NetworkServer.Spawn(blackHole);
            }

            CreateLine(aimPosition, hit.point);           
        }
    }
}
