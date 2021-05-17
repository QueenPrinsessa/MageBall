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

            if (Input.GetButtonDown("Fire1") && UseMana(mainSpell.ManaCost))
                mainSpell.CmdCastSpell();
            else if (Input.GetButtonDown("Fire2") && UseMana(offhandSpell.ManaCost))
                offhandSpell.CmdCastSpell();
            else if (Input.GetButtonDown("Fire3") && UseMana(extraSpell.ManaCost))
                extraSpell.CmdCastSpell();
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
                case Spells.Magnify:
                    return GetComponent<MagnifySpell>();
                case Spells.Minimize:
                    return GetComponent<MinimizeSpell>();
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
