using UnityEngine;
using Universal;
using ShipIt.Gameplay.Astral;

namespace ShipIt.Gameplay
{
    public class GameplayManager : Singleton<GameplayManager>
    {
        [SerializeField] FuelBank fuelBank;
        [SerializeField] OrderManager orderManager;

        public bool OrderCompleted { get; private set; }
        public bool OrderFailed { get; private set; }

        public System.Action OnOrderCompleted;
        public System.Action OnOrderFailed;

        internal override void Awake()
        {
            base.Awake();

            if (inst != this)
            {
                return;
            }

            if (!orderManager)
            {
                orderManager = OrderManager.inst;
            }

            if (!fuelBank)
            {
                fuelBank = FindObjectOfType<FuelBank>();
            }
        }

        void OnEnable()
        {
            Subscribe();
        }

        void OnDisable()
        {
            Unsubscribe();
        }

        void Subscribe()
        {
            if (orderManager != null)
            {
                orderManager.TargetReached += HandleOrderCompleted;
            }

            if (fuelBank != null)
            {
                fuelBank.OnFuelDepleted += HandleOrderFailed;
            }
        }

        void Unsubscribe()
        {
            if (orderManager != null)
            {
                orderManager.TargetReached -= HandleOrderCompleted;
            }

            if (fuelBank != null)
            {
                fuelBank.OnFuelDepleted -= HandleOrderFailed;
            }
        }

        void HandleOrderCompleted()
        {
            if (OrderCompleted || OrderFailed)
            {
                return;
            }

            OrderCompleted = true;
            PauseGameplay();
            OnOrderCompleted?.Invoke();
        }

        void HandleOrderFailed()
        {
            if (OrderFailed || OrderCompleted)
            {
                return;
            }

            OrderFailed = true;
            PauseGameplay();
            OnOrderFailed?.Invoke();
        }

        static void PauseGameplay() => Time.timeScale = 0f;
    }
}
