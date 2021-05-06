using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MageBall
{
    public struct PlayerLoadout
    {
        public PlayerLoadout(Spells mainSpell, Spells offhandSpell, Spells extraSpell, PlayerModel playerModel, Team team)
        {
            MainSpell = mainSpell;
            OffhandSpell = offhandSpell;
            ExtraSpell = extraSpell;
            PlayerModel = playerModel;
            Team = team;
        }

        public Spells MainSpell { get; private set; }
        public Spells OffhandSpell { get; private set; }
        public Spells ExtraSpell { get; private set; }
        public PlayerModel PlayerModel { get; private set; }
        public Team Team { get; private set; }
    }
}