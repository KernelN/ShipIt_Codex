using TMPro;
using UnityEngine;

namespace ShipIt
{
    public class FuelUI : MonoBehaviour
    {
        [SerializeField] FuelManager fuelManager;
        [SerializeField] FuelBank fuelBank;
        [SerializeField] TextMeshProUGUI fuelLabel;

        void Awake()
        {
            if (fuelManager == null && fuelBank == null)
            {
                return;
            }

            Subscribe();
            RefreshFuel();
        }

        void OnDisable()
        {
            Unsubscribe();
        }

        void OnDestroy()
        {
            Unsubscribe();
        }

        void Subscribe()
        {
            if (fuelManager != null)
            {
                fuelManager.OnFuelChanged += HandleFuelChanged;
            }

            if (fuelBank != null)
            {
                fuelBank.OnFuelChanged += HandleFuelChanged;
            }
        }

        void Unsubscribe()
        {
            if (fuelManager != null)
            {
                fuelManager.OnFuelChanged -= HandleFuelChanged;
            }

            if (fuelBank != null)
            {
                fuelBank.OnFuelChanged -= HandleFuelChanged;
            }
        }

        void RefreshFuel()
        {
            if (fuelManager == null && fuelBank == null)
            {
                return;
            }

            int amount = 0;

            if (fuelManager != null)
            {
                amount = fuelManager.GetCurrentFuel();
            }
            else if (fuelBank != null)
            {
                amount = fuelBank.CurrentFuel;
            }

            HandleFuelChanged(amount);
        }

        void HandleFuelChanged(int amount)
        {
            if (fuelLabel != null)
            {
                fuelLabel.text = amount.ToString();
            }
        }
    }
}
