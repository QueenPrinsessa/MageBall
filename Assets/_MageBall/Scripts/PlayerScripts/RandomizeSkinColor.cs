using Mirror;
using UnityEngine;

namespace MageBall
{
    /// <summary>
    /// Huvudansvarig: Zoe
    /// </summary>
    public class RandomizeSkinColor : NetworkBehaviour
    {
        [SerializeField] private new Renderer renderer;
        [SerializeField] private int skinMaterialSlot;
        [SerializeField] private Material[] skinMaterials;
        [SyncVar(hook = nameof(OnCurrentSkinIndexChanged))]
        private int currentSkinIndex = -1;

        public override void OnStartServer()
        {
            currentSkinIndex = Random.Range(0, skinMaterials.Length);
        }

        private void OnCurrentSkinIndexChanged(int oldSkinIndex, int newSkinIndex)
        {
            Material[] materials = renderer.materials;
            materials[skinMaterialSlot] = skinMaterials[newSkinIndex];
            renderer.materials = materials;
        }
    }
}
