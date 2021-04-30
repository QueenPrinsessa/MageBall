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
        [SerializeField] private Passive increasedMana;
        private Animator animator;
        private float maxMana = 100f;
        private float manaAmount;
        private float rechargeRate = 20f;

        public override void OnStartAuthority()
        {
            animator = GetComponent<Animator>();
            maxMana *= increasedMana.modifier;
            manaAmount = maxMana;
        }

        [Client]
        void Update()
        {
            if (!hasAuthority)
                return;

            manaAmount += rechargeRate * Time.deltaTime;
            manaAmount = Mathf.Clamp(manaAmount, 0f, maxMana);

            if (Input.GetButtonDown("Fire1") && UseMana(mainSpell.ManaCost))
            {
                mainSpell.CmdCastSpell();
                animator.SetTrigger("Attack1");
            }

            if (Input.GetButtonDown("Fire2") && UseMana(offhandSpell.ManaCost))
            {
                offhandSpell.CmdCastSpell();
                animator.SetTrigger("Attack2");
            }

            if (Input.GetButtonDown("Fire3") && UseMana(thirdSpell.ManaCost))
            {
                thirdSpell.CmdCastSpell();
                animator.SetTrigger("Attack2");
            }

        }

        private bool UseMana(float amount)
        {
            if (manaAmount < amount)
                return false;

            manaAmount -= amount;
            return true;
        }

        public float GetManaNormalized()
        {
            return manaAmount / maxMana;
        }

        public void ResetMana()
        {
            manaAmount = maxMana;
        }

    }
}
