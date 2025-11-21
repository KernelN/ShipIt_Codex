using UnityEngine;

namespace ShipIt
{
    public class FuelBank : MonoBehaviour
    {
        [SerializeField] int launchFuelCost = 1;
        int cachedFuel;

        public bool IsFuelDepleted { get; private set; }
        public int CurrentFuel => cachedFuel;
        public System.Action<int> OnFuelChanged;
        public System.Action OnFuelDepleted;

        GameManager gameManager;

        void Start()
        {
            gameManager = GameManager.inst;
            if (gameManager != null)
            {
                GameData data = gameManager.Data;
                cachedFuel = data.fuel;
            }

            NotifyFuelChanged();

            if (cachedFuel <= 0)
            {
                FlagDepleted();
            }
            if (!HasFuelToConsume()) FlagDepleted();
        }

        public bool TryConsumeForLaunch()
        {
            if (launchFuelCost <= 0) return true;

            if (!HasFuelToConsume())
            {
                FlagDepleted();
                return false;
            }

            cachedFuel -= launchFuelCost;
            PersistFuel();

            NotifyFuelChanged();

            if (cachedFuel <= 0)
            {
                FlagDepleted();
            }

            return true;
        }

        bool HasFuelToConsume() => cachedFuel >= launchFuelCost;

        void PersistFuel()
        {
            if (gameManager == null)
            {
                return;
            }

            GameData data = gameManager.Data;
            data.fuel = cachedFuel;
            gameManager.SaveGameData();
        }

        void FlagDepleted()
        {
            if (IsFuelDepleted)
            {
                return;
            }

            IsFuelDepleted = true;
            OnFuelDepleted?.Invoke();
        }

        void NotifyFuelChanged()
        {
            OnFuelChanged?.Invoke(cachedFuel);
        }

        void OnDestroy()
        {
            PersistFuel();
        }
    }
}