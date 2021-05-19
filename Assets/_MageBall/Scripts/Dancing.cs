using Mirror;
using UnityEngine;

namespace MageBall
{
    public class Dancing : NetworkBehaviour
    {
        private Animator animator;
        private bool isDance1Locked;
        private bool isDance2Locked;
        private bool isDance3Locked;
        private bool isDance4Locked;

        public override void OnStartAuthority()
        {
            animator = GetComponent<Animator>();
        }

        private void Update()
        {
            if (!hasAuthority)
                return;

            bool dance1 = Input.GetAxisRaw("DpadLeftRight") == 1;
            bool dance2 = Input.GetAxisRaw("DpadUpDown") == 1;
            bool dance3 = Input.GetAxisRaw("DpadLeftRight") == -1;
            bool dance4 = Input.GetAxisRaw("DpadUpDown") == -1;

            if (!dance1)
                isDance1Locked = false;
            if (!dance2)
                isDance2Locked = false;
            if (!dance3)
                isDance3Locked = false;
            if (!dance4)
                isDance4Locked = false;

            if (dance1 && !isDance1Locked)
            {
                StopDancing();
                animator.SetBool("Dance1", !animator.GetBool("Dance1"));
                isDance1Locked = true;
            }
            else if (dance2 && !isDance2Locked)
            {
                StopDancing();
                animator.SetBool("Dance2", !animator.GetBool("Dance2"));
                isDance2Locked = true;
            }
            else if (dance3 && !isDance3Locked)
            {
                StopDancing();
                animator.SetBool("Dance3", !animator.GetBool("Dance3"));
                isDance3Locked = true;
            }
            else if (dance4 && !isDance4Locked)
            {
                StopDancing();
                animator.SetBool("Dance4", !animator.GetBool("Dance4"));
                isDance4Locked = true;
            }

        }

        private void StopDancing()
        {
            animator.SetBool("Dance1", false);
            animator.SetBool("Dance2", false);
            animator.SetBool("Dance3", false);
            animator.SetBool("Dance4", false);
        }

    }
}