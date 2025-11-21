using UnityEngine;

namespace ShipIt.Gameplay.Astral
{
    [CreateAssetMenu(menuName = "ShipIt/AstralComponent/Target", fileName = "AstralTarget")]
    public class AstralTargetBuilder : AstralComponentBuilder
    {
        public override AstralComponentType GetType => AstralComponentType.Target;

        public override AstralComponent GetComponent() => new AstralTarget();
    }
}
