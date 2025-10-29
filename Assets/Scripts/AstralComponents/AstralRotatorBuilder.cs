using UnityEngine;

namespace ShipIt.Gameplay.Astral
{
    [CreateAssetMenu(menuName = "Astral/Rotator", fileName = "AstralRotator")]
    public class AstralRotatorBuilder : AstralComponentBuilder
    {
        [SerializeField] float minRotSpeed = -30f;
        [SerializeField] float maxRotSpeed = 30f;

        public override AstralComponentType GetType => AstralComponentType.Rotator;

        public override AstralComponent GetComponent() 
            => new AstralRotator(Random.Range(minRotSpeed, maxRotSpeed));
    }
}
