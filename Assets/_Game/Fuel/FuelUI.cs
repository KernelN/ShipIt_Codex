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
            if (!fuelManager && !fuelBank) return;

            Subscribe();
            RefreshFuel();
        }
        void OnDestroy() => Unsubscribe();

        void Subscribe()
        {
            if (fuelManager)
                fuelManager.OnFuelChanged += HandleFuelChanged;

            if (fuelBank)
                fuelBank.OnFuelChanged += HandleFuelChanged;
        }
        void Unsubscribe()
        {
            if (fuelManager) 
                fuelManager.OnFuelChanged -= HandleFuelChanged;

            if (fuelBank) 
                fuelBank.OnFuelChanged -= HandleFuelChanged;
        }
        void RefreshFuel()
        {
            if (!fuelManager && !fuelBank) return;

            int amount = 0;

            if (fuelManager)
                amount = fuelManager.CurrentFuel;
            else if (fuelBank) 
                amount = fuelBank.CurrentFuel;

            HandleFuelChanged(amount);
        }
        void HandleFuelChanged(int amount)
        {
            if (fuelLabel) 
                fuelLabel.text = amount.ToString();
        }
    }
}
