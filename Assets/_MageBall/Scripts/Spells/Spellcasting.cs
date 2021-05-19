using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace MageBall
{ 
    public class Spellcasting : NetworkBehaviour
    {
        [SerializeField] private Passive increasedMana;
        [SerializeField] private float maxMana = 100f;
        [SerializeField] private float baseRechargeRate = 20f;
        [SyncVar] private Spell mainSpell;
        [SyncVar] private Spell offhandSpell;
        [SyncVar] private Spell extraSpell;
        [SyncVar] private Passives currentPassive;
        private float currentMana;
        private bool isMainSpellLocked;
        private bool isOffhandSpellLocked;
        private bool isExtraSpellLocked;

        [SyncVar]
        private bool canCastSpells = true;

        public float ManaNormalized
        {
            get 
            { 
                return currentMana / maxMana; 
            }
        }

        public float RechargeRate
        {
            get => currentPassive == Passives.FasterManaRegen ? baseRechargeRate * increasedMana.modifier : baseRechargeRate;
        }

        public override void OnStartAuthority()
        {
            ResetMana();
        }

        [ClientCallback]
        void Update()
        {
            if (!hasAuthority)
                return;

            currentMana += RechargeRate * Time.deltaTime;
            currentMana = Mathf.Clamp(currentMana, 0f, maxMana);

            if (!canCastSpells)
                return;

            bool mainSpellCast = (Input.GetButton("Fire1") || Input.GetAxisRaw("Fire1Trigger") == 1);
            bool offhandSpellCast = (Input.GetButton("Fire2") || Input.GetAxisRaw("Fire2Trigger") == 1);
            bool extraSpellCast = Input.GetButton("Fire3");

            if (!mainSpellCast)
                isMainSpellLocked = false;

            if (!offhandSpellCast)
                isOffhandSpellLocked = false;

            if (!extraSpellCast)
                isExtraSpellLocked = false;

            if (mainSpellCast && !isMainSpellLocked && UseMana(mainSpell.ManaCost))
            {
                mainSpell.CmdCastSpell();
                isMainSpellLocked = true;
            }
            else if (offhandSpellCast && !isOffhandSpellLocked && UseMana(offhandSpell.ManaCost))
            {
                offhandSpell.CmdCastSpell();
                isOffhandSpellLocked = true;
            }
            else if (extraSpellCast && !isExtraSpellLocked && UseMana(extraSpell.ManaCost))
            {
                extraSpell.CmdCastSpell();
                isExtraSpellLocked = true;
            }
            
        }

        [Server]
        public void SetPlayerLoadout(PlayerLoadout playerLoadout)
        {
            mainSpell = GetSpell(playerLoadout.MainSpell);
            offhandSpell = GetSpell(playerLoadout.OffhandSpell);
            extraSpell = GetSpell(playerLoadout.ExtraSpell);
            currentPassive = playerLoadout.Passive;
        }

        private Spell GetSpell(Spells spell)
        {
            switch (spell)
            {
                case Spells.ForcePush:
                    return GetComponent<ForcePush>();
                case Spells.ForcePull:
                    return GetComponent<ForcePull>();
                case Spells.ForceFly:
                    return GetComponent<ForceFly>();
                case Spells.Boxmancy:
                    return GetComponent<Boxmancy>();
                case Spells.Maximize:
                    return GetComponent<Maximize>();
                case Spells.Minimize:
                    return GetComponent<Minimize>();
                default:
                    return GetComponent<ForcePush>();
            }
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

        public void ResetMana()
        {
            currentMana = maxMana;
        }

        [TargetRpc]
        public void TargetEnablePowerUp(int powerUpDuration)
        {
            StartCoroutine(EnableUnlimitedCasting(powerUpDuration));
        }

        private IEnumerator EnableUnlimitedCasting(int powerUpDuration)
        {
            float defaultRechargeRate = baseRechargeRate;
            baseRechargeRate = maxMana * 3;
            yield return new WaitForSeconds(powerUpDuration);
            baseRechargeRate = defaultRechargeRate;
        }

    }
}
