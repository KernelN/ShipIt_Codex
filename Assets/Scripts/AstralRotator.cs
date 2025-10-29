using UnityEngine;

namespace ShipIt.Gameplay.Astral
{
    public class AstralRotator : AstralComponent
    {
        readonly float speed;
        Transform target;

        public AstralRotator(float speed)
        {
            this.speed = speed;
        }

        public override void Set(AstralBody body)
        {
            target = body != null ? body.transform : null;
        }

        public override void Update()
        {
            if (target == null)
                return;

            target.Rotate(Vector3.up, speed * Time.deltaTime, Space.Self);
        }
    }
}
