using TMPro;
using UnityEngine;

namespace ShipIt
{
    public class FuelUI : MonoBehaviour
    {
        [SerializeField] FuelManager fuelManager;
        [SerializeField] FuelBank fuelBank;
        [SerializeField] TextMeshProUGUI fuelLabel;

        void OnEnable()
        {
            if (!fuelManager)
            {
                fuelManager = FindObjectOfType<FuelManager>();
            }

            if (!fuelBank)
            {
                fuelBank = FindObjectOfType<FuelBank>();
            }

            Subscribe();
            RefreshFuel();
        }

        void OnDisable()
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
