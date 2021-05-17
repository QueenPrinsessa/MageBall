using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MageBall
{
    public class Resizable : NetworkBehaviour
    {

        [SerializeField] private uint maxMagnifyStacks = 5;
        [SerializeField] private uint maxMinimizeStacks = 5;

        [SyncVar] private Vector3 defaultScale;
        [SyncVar] private Vector3 currentScale;

        private readonly SyncList<SpellStackInfo> stacks = new SyncList<SpellStackInfo>();

        public override void OnStartServer()
        {
            defaultScale = transform.localScale;
            currentScale = defaultScale;
            stacks.Callback += UpdateScale;
        }

        private void UpdateScale(SyncList<SpellStackInfo>.Operation op, int itemIndex, SpellStackInfo oldItem, SpellStackInfo newItem)
        {
            float combinedModifier = 1f;

            foreach (SpellStackInfo stack in stacks)
            {
                combinedModifier *= stack.Modifier;
            }

            currentScale = defaultScale * combinedModifier;
        }

        [Server]
        public void Magnify(SpellStackInfo info)
        {
            stacks.Add(info);
            StartCoroutine(RemoveStack(info));
        }

        private IEnumerator RemoveStack(SpellStackInfo info)
        {
            yield return new WaitForSeconds(info.Duration);
            stacks.Remove(info);
            
        }

        [Server]
        public void Minimize(float modifier)
        {

        }
    }
}