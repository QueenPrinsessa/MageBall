using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

namespace MageBall
{
    public class ForceFly : MonoBehaviour
    {
        void Update()
        {
            if(Input.GetKeyDown(KeyCode.F)) //(Input.GetButtonDown("Fire1"))
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
        public void CastSpell()
        {
            if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, Mathf.Infinity, LayerMasks.ballLayer))
            {
                if (hit.rigidbody != null)
                {
                    if(hit.rigidbody.useGravity == true)
                    {
                        hit.rigidbody.useGravity = false;
                        StartCoroutine(Timer());
                    }
                }
            }
        }
        private IEnumerator Timer()
        {
            yield return new WaitForSeconds(5);
            GameObject ball = GameObject.FindGameObjectWithTag(Tags.BallTag);
            Rigidbody rigidbody = ball.GetComponent<Rigidbody>();
            rigidbody.useGravity = true;
        }
    }
}
