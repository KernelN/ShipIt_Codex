namespace ShipIt.Gameplay.Astral
{
    public class AstralTarget : AstralComponent
    {
        AstralBody targetBody;

        public override void Set(AstralBody body)
        {
            if (targetBody != null)
            {
                targetBody.onShipEntered -= HandleShipEntered;
            }

            targetBody = body;

            if (targetBody != null)
            {
                targetBody.onShipEntered += HandleShipEntered;
            }
        }

        void HandleShipEntered(Ship ship)
        {
            AstralManager.inst?.OnTargetReached();
        }
    }
}
