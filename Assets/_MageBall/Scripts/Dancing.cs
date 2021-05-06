using Mirror;
using UnityEngine;

namespace MageBall
{
    public class Dancing : NetworkBehaviour
    {
        private Animator animator;

        public override void OnStartAuthority()
        {
            animator = GetComponent<Animator>();
        }

        private void Update()
        {
            if (!hasAuthority)
                return;

            if (Input.GetKeyDown("j"))
            {
                animator.SetBool("Dance1", !animator.GetBool("Dance1"));
                animator.SetBool("Dance2", false);
                animator.SetBool("Dance3", false);
            }
            else if (Input.GetKeyDown("k"))
            {
                animator.SetBool("Dance1", false);
                animator.SetBool("Dance2", !animator.GetBool("Dance2"));
                animator.SetBool("Dance3", false);
            }
            else if (Input.GetKeyDown("l"))
            {
                animator.SetBool("Dance1", false);
                animator.SetBool("Dance2", false);
                animator.SetBool("Dance3", !animator.GetBool("Dance3"));
            }
        }

    }
}