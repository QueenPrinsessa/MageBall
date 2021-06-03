using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MageBall
{
    public class Resizable : NetworkBehaviour
    {

        private readonly uint maxMagnifyStacks = 5;
        private readonly uint maxMinimizeStacks = 5;
        private uint magnifyStacks = 0;
        private uint minimizeStacks = 0;

        private Vector3 defaultScale;

        private readonly List<SpellStackInfo> stacks = new List<SpellStackInfo>();

        public override void OnStartServer()
        {
            defaultScale = transform.localScale;
        }

        [Server]
        private void RecalculateScale()
        {
            float combinedModifier = 1f;

            foreach (SpellStackInfo stack in stacks)
            {
                combinedModifier *= stack.Modifier;
            }

            transform.localScale = defaultScale * combinedModifier;
        }

        [Server]
        public void ApplySpell(SpellStackInfo info)
        {
            switch (info.Spell)
            {
                case Spells.Maximize:
                    magnifyStacks++;

                    if (magnifyStacks > maxMagnifyStacks)
                    {
                        magnifyStacks = maxMinimizeStacks;
                        return;
                    }

                    break;
                case Spells.Minimize:
                    minimizeStacks++;

                    if (minimizeStacks > maxMinimizeStacks)
                    {
                        minimizeStacks = maxMinimizeStacks;
                        return;
                    }

                    break;
                default:
                    Debug.LogWarning($"Attempted to pass non-resize spell {info.Spell} to Resizable script.");
                    return;
            }

            stacks.Add(info);
            StartCoroutine(RemoveStack(info));
            RecalculateScale();
        }

        [Server]
        private IEnumerator RemoveStack(SpellStackInfo info)
        {
            yield return new WaitForSeconds(info.Duration);
            stacks.Remove(info);
            DecreaseStackCount(info);
            RecalculateScale();
        }

        [Server]
        private void DecreaseStackCount(SpellStackInfo stack)
        {
            switch (stack.Spell)
            {
                case Spells.Maximize:
                    magnifyStacks--;
                    break;
                case Spells.Minimize:
                    minimizeStacks--;
                    break;
            }
        }

    }
}