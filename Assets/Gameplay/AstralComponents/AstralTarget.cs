using ShipIt.Gameplay;

namespace ShipIt.Gameplay.Astral
{
    public class AstralTarget : AstralComponent
    {
        AstralBody owner;

        public override void Set(AstralBody body)
        {
            owner = body;
            owner.onShipEntered += HandleShipEntered;
        }

        void HandleShipEntered(Ship ship)
        {
            if (owner != null)
            {
                owner.onShipEntered -= HandleShipEntered;
            }

            OrderManager.inst?.OnTargetReached();
        }
    }
}
