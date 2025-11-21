using UnityEngine;
using ShipIt.Gameplay;
using ShipIt.TickManaging;

namespace ShipIt.Gameplay.Astral
{
    public class AstralHazard : AstralComponent
    {
        readonly float damageInterval;
        readonly float damagePerTick;
        AstralBody owner;
        Ship trackedShip;
        bool isSubscribed;

        public AstralHazard(float damageInterval, float damagePerTick)
        {
            this.damageInterval = Mathf.Max(0.01f, damageInterval);
            this.damagePerTick = damagePerTick;
        }

        public override void Set(AstralBody body)
        {
            owner = body;
            owner.onShipEntered += HandleShipEntered;
            owner.onShipExit += HandleShipExit;
        }

        void HandleShipEntered(Ship ship)
        {
            trackedShip = ship;
            SuscribeDamageTick();
        }

        void HandleShipExit(Ship ship)
        {
            if (ship != trackedShip)
                return;

            trackedShip = null;
            UnsuscribeDamageTick();
        }

        void SuscribeDamageTick()
        {
            if (isSubscribed || UpdateManager.inst == null)
                return;

            UpdateManager.inst.SuscribeToScaled(damageInterval, DealDamage);
            isSubscribed = true;
        }

        void UnsuscribeDamageTick()
        {
            if (!isSubscribed || UpdateManager.inst == null)
                return;

            UpdateManager.inst.RemoveFromScaled(damageInterval, DealDamage);
            isSubscribed = false;
        }

        void DealDamage()
        {
            if (trackedShip == null)
            {
                UnsuscribeDamageTick();
                return;
            }

            trackedShip.ApplyDamage(damagePerTick);
        }
    }
}
