using UnityEngine;

namespace MageBall
{
    public static class LayerMasks
    {

        public static readonly LayerMask ballLayer;
        public static readonly LayerMask playerLayer;
        public static readonly LayerMask groundLayer;
        public static readonly LayerMask spellsLayer;

        static LayerMasks()
        {
            ballLayer = LayerMask.GetMask("Ball");
            playerLayer = LayerMask.GetMask("Player");
            groundLayer = LayerMask.GetMask("Ground");
            spellsLayer = LayerMask.GetMask("Spells");
        }
    }
}