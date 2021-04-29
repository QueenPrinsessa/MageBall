using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace MageBall
{ 
    public class Spellcasting : NetworkBehaviour
    {
        [SerializeField] private Spell mainSpell;
        [SerializeField] private Spell offhandSpell;
        [SerializeField] private Spell thirdSpell;
        private Animator animator;
        private float maxMana = 100f;
        private float manaAmount = 0f;
        private float spellCost = 30f;
        private float rechargeRate = 20f;

        public override void OnStartAuthority()
        {
            animator = GetComponent<Animator>();

        }

        [Client]
        void Update()
        {
            if (!hasAuthority)
                return;

            manaAmount += rechargeRate * Time.deltaTime;

            if (Input.GetButtonDown("Fire1"))
            {
                mainSpell.CmdCastSpell();
                StartCoroutine(Attack1());
                useMana(30);
            }

            if (Input.GetButtonDown("Fire2"))
            {
                offhandSpell.CmdCastSpell();
                StartCoroutine(Attack2());
                useMana(30);
            }

            if (Input.GetButtonDown("Fire3"))
            {
                thirdSpell.CmdCastSpell();
                StartCoroutine(Attack2());
                useMana(30);
            }

        }

        private void useMana(int amount)
        {
            if (manaAmount >= amount)
                manaAmount -= amount;
        }

        public float GetManaNomralized()
        {
            return manaAmount / maxMana;
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
