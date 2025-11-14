using UnityEngine;

namespace ShipIt.Gameplay.Astral
{
    [CreateAssetMenu(menuName = "ShipIt/AstralComponent/Rotator", fileName = "AstralRotator")]
    public class AstralRotatorBuilder : AstralComponentBuilder
    {
        [SerializeField, Min(0)] float minRotSpeed = 15f;
        [SerializeField, Min(0)] float maxRotSpeed = 30f;

        public override AstralComponentType GetType => AstralComponentType.Rotator;

        public override AstralComponent GetComponent()
        {
            int sign = Random.value > .5f ? 1 : -1;
            return new AstralRotator(Random.Range(minRotSpeed, maxRotSpeed) * sign);
        }
    }
}
