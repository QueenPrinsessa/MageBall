using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MageBall
{
    public class SpellStackInfo
    {

        public SpellStackInfo() {}

        public SpellStackInfo(Spells spell, float modifier, float duration)
        {
            Spell = spell;
            Modifier = modifier;
            Duration = duration;
        }

        public Spells Spell { get; private set; }

        public float Modifier { get; private set; }
        
        public float Duration { get; private set; }
    }
}