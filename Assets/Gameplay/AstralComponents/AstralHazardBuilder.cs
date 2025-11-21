using UnityEngine;

namespace ShipIt.Gameplay.Astral
{
    [CreateAssetMenu(menuName = "ShipIt/AstralComponent/Hazard", fileName = "AstralHazard")]
    public class AstralHazardBuilder : AstralComponentBuilder
    {
        [SerializeField, Min(0.01f)] float damageInterval = 1f;
        [SerializeField, Min(0f)] float damagePerTick = 10f;

        public override AstralComponentType GetType => AstralComponentType.Hazard;

        public override AstralComponent GetComponent()
        {
            return new AstralHazard(damageInterval, damagePerTick);
        }
    }
}
