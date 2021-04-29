using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace MageBall
{ 
    public class Spellcasting : NetworkBehaviour
    {
        [SerializeField] private Spell mainSpell;
        [SerializeField] private Spell offhandSpell;
        [SerializeField] private Spell thirdSpell;
        private Animator animator;

        public override void OnStartAuthority()
        {
            animator = GetComponent<Animator>();
        }

        [Client]
        void Update()
        {
            if (!hasAuthority)
                return;

            if (Input.GetButtonDown("Fire1"))
            {
                mainSpell.CmdCastSpell();
                animator.SetTrigger("Attack1");
            }

            if (Input.GetButtonDown("Fire2"))
            {
                offhandSpell.CmdCastSpell();
                animator.SetTrigger("Attack2");
            }

            if (Input.GetButtonDown("Fire3"))
            {
                thirdSpell.CmdCastSpell();
                animator.SetTrigger("Attack2");
            }
        }

    }
}
