using System;
using UnityEngine;

namespace ShipIt
{
    public class FuelBank : MonoBehaviour
    {
        [SerializeField] int launchFuelCost = 1;
        [SerializeField, Min(0)] int cachedFuel;

        public bool IsFuelDepleted { get; private set; }
        public int CurrentFuel => cachedFuel;
        public event Action<int> OnFuelChanged;
        public event Action OnFuelDepleted;

        GameManager gameManager;

        void Start()
        {
            gameManager = GameManager.inst;
            if (gameManager != null)
            {
                GameData data = gameManager.Data;
                cachedFuel = Mathf.Max(0, data.fuel);
            }

            NotifyFuelChanged();

            if (cachedFuel <= 0)
            {
                FlagDepleted();
            }
        }

        public bool TryConsumeForLaunch()
        {
            if (launchFuelCost <= 0)
            {
                return true;
            }

            if (cachedFuel < launchFuelCost)
            {
                if (cachedFuel <= 0)
                {
                    FlagDepleted();
                }
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
