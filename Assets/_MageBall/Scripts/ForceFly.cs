using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

namespace MageBall
{
    public class ForceFly : Spell
    {
        [Command]
        public override void CmdCastSpell()
        {
            if (Physics.Raycast(aimPosition, aimForward, out RaycastHit hit, Mathf.Infinity, LayerMasks.ballLayer))
            {
                if (hit.rigidbody != null)
                {
                    if(hit.rigidbody.useGravity == true)
                    {
                        hit.rigidbody.useGravity = false;
                        StartCoroutine(EnableGravity());
                    }
                }
            }
        }
        private IEnumerator EnableGravity()
        {
            yield return new WaitForSeconds(5);
            GameObject ball = GameObject.FindGameObjectWithTag(Tags.BallTag);
            Rigidbody rigidbody = ball.GetComponent<Rigidbody>();
            rigidbody.useGravity = true;
        }
    }
}
