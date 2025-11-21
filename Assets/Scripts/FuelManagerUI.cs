using TMPro;
using UnityEngine;

namespace ShipIt
{
    public class FuelManagerUI : MonoBehaviour
    {
        [SerializeField] FuelManager fuelManager;
        [SerializeField] TextMeshProUGUI fuelLabel;

        void OnEnable()
        {
            if (!fuelManager)
            {
                fuelManager = FindObjectOfType<FuelManager>();
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
            if (fuelManager == null)
            {
                return;
            }

            fuelManager.OnFuelChanged += HandleFuelChanged;
        }

        void Unsubscribe()
        {
            if (fuelManager == null)
            {
                return;
            }

            fuelManager.OnFuelChanged -= HandleFuelChanged;
        }

        void RefreshFuel()
        {
            HandleFuelChanged(fuelManager != null ? fuelManager.GetCurrentFuel() : 0);
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
