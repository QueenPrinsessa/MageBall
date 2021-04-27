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
                StartCoroutine(Attack1());
            }

            if (Input.GetButtonDown("Fire2"))
            {
                offhandSpell.CmdCastSpell();
                StartCoroutine(Attack2());
            }

            if (Input.GetButtonDown("Fire3"))
            {
                thirdSpell.CmdCastSpell();
                StartCoroutine(Attack2());
            }
        }

        private IEnumerator Attack1()
        {
            animator.SetBool("Attack1", true);

            yield return new WaitForSeconds(1);
            
            animator.SetBool("Attack1", false);
        }

        private IEnumerator Attack2()
        {
            animator.SetBool("Attack2", true);

            yield return new WaitForSeconds(1);

            animator.SetBool("Attack2", false);
        }
    }
}
