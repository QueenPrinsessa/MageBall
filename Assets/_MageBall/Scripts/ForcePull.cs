using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
namespace MageBall
{

    public class ForcePull : Spell
    {

        [SerializeField]
        private float modifier = 10.0f;

        private void Update()
        {
            if (Input.GetButtonDown("Fire1"))
            {
                CastSpell();
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Ray ray = new Ray(transform.position, transform.forward);
            Gizmos.DrawRay(ray);
        }

        public override void CastSpell()
        {
            if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, Mathf.Infinity, LayerMasks.ballLayer))
            {
                Vector3 pullDirection = transform.position - hit.transform.position;
                Vector3 pullForce = pullDirection.normalized * modifier;
                if (hit.rigidbody != null)
                    hit.rigidbody.AddForce(pullForce, ForceMode.Impulse);
            }
        }
    }
}