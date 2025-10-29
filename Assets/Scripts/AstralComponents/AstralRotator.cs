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
            target = body.transform;
            body.OnUpdate += Update;
        }
        public void Update(float dt, float udt)
        {
            target.Rotate(Vector3.up, speed * dt);
        }
    }
}