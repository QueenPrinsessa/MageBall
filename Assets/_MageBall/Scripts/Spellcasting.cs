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
        [SerializeField]private float maxMana = 100f;
        [SerializeField]private float rechargeRate = 20f;
        private float currentMana;

        [SyncVar]
        private bool canCastSpells = true;

        public override void OnStartAuthority()
        {
            maxMana *= increasedMana.modifier;
            currentMana = maxMana;
        }

        [ClientCallback]
        void Update()
        {
            if (!hasAuthority)
                return;

            currentMana += rechargeRate * Time.deltaTime;
            currentMana = Mathf.Clamp(currentMana, 0f, maxMana);

            if (!canCastSpells)
                return;

            if (Input.GetButtonDown("Fire1") && UseMana(mainSpell.ManaCost))
                mainSpell.CmdCastSpell();
            else if (Input.GetButtonDown("Fire2") && UseMana(offhandSpell.ManaCost))
                offhandSpell.CmdCastSpell();
            else if (Input.GetButtonDown("Fire3") && UseMana(thirdSpell.ManaCost))
                thirdSpell.CmdCastSpell();
        }

        [Command]
        public void CmdSetCanCastSpells(bool canCastSpells)
        {
            this.canCastSpells = canCastSpells;
        }

        private bool UseMana(float amount)
        {
            if (currentMana < amount)
                return false;

            currentMana -= amount;
            return true;
        }

        public float ManaNormalized
        {
            get { return currentMana / maxMana; }
        }

        public void ResetMana()
        {
            currentMana = maxMana;
        }

    }
}
