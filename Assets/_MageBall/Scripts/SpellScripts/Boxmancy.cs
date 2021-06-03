using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MageBall
{
    public class Boxmancy : Spell
    {

        [Header("Boxmancy Settings")]
        [SerializeField] private GameObject boxPrefab;
        [SerializeField] private float force = 15f;

        [Command]
        public override void CmdCastSpell()
        {
            TargetTriggerAttackAnimation("Attack1");

            GameObject box = Instantiate(boxPrefab, aimPosition, aimRotation);
            NetworkServer.Spawn(box);
            Rigidbody rigidbody = box.GetComponent<Rigidbody>();

            if (rigidbody == null)
                return;

            rigidbody.AddForce(aimForward * force, ForceMode.Impulse);

            //add destruction of boxes or disintegration after a while
        }
    }
}