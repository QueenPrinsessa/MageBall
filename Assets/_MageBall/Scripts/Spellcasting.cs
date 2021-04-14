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

        [Client]
        void Update()
        {
            if (!hasAuthority)
                return;

            if (Input.GetButtonDown("Fire1"))
            {
                mainSpell.CmdCastSpell();
            }

            if (Input.GetButtonDown("Fire2"))
            {
                offhandSpell.CmdCastSpell();
            }

            if (Input.GetButtonDown("Fire3"))
            {
                thirdSpell.CmdCastSpell();
            }
        }
    }
}