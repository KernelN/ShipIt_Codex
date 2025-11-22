using UnityEngine;

namespace ShipIt.Gameplay.Astral
{
    [CreateAssetMenu(menuName = "ShipIt/AstralComponent/Target", fileName = "AstralTarget")]
    public class AstralTargetBuilder : AstralComponentBuilder
    {
        [SerializeField] GameObject outline;
        public override AstralComponentType GetType => AstralComponentType.Target;
        public void Build(Transform target)
        {
            Instantiate(outline, target.position, Quaternion.identity, target);
        }
        public override AstralComponent GetComponent() => new AstralTarget();
    }
}
