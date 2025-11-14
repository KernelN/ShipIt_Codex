using ShipIt.Gameplay;
using UnityEngine;

namespace ShipIt.Gameplay.Astral
{
    [CreateAssetMenu(fileName = "PlanetFactory", menuName = "ShipIt/Astral/Planet Factory")]
    public class PlanetFactory : AstralBodyFactory
    {
        [SerializeField] Material[] materials;

        public override AstralBody SpawnBody(Vector3 position, Quaternion rotation, Transform parent = null)
        {
            ComponentExposer exposer = CreateBody(position, rotation, parent);
            if (exposer == null)
            {
                return null;
            }

            ApplyRandomMaterial(exposer.Renderer);
            return exposer.AstralBody;
        }

        void ApplyRandomMaterial(Renderer renderer)
        {
            if (renderer == null || materials == null || materials.Length == 0)
            {
                return;
            }

            int index = Random.Range(0, materials.Length);
            renderer.sharedMaterial = materials[index];
        }
    }
}
