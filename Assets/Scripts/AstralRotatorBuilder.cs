using UnityEngine;

namespace ShipIt.Gameplay.Astral
{
    [CreateAssetMenu(menuName = "Astral/Rotator", fileName = "AstralRotator")]
    public class AstralRotatorBuilder : AstralComponentBuilder
    {
        [SerializeField]
        float minRotSpeed = 10f;

        [SerializeField]
        float maxRotSpeed = 30f;

        public override AstralComponentType GetType => AstralComponentType.Rotator;

        public override AstralComponent GetComponent()
        {
            var min = Mathf.Min(minRotSpeed, maxRotSpeed);
            var max = Mathf.Max(minRotSpeed, maxRotSpeed);

            var speed = Random.Range(min, max);
            return new AstralRotator(speed);
        }
    }
}
