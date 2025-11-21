using TMPro;
using UnityEngine;

namespace ShipIt
{
    public class CreditsUI : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI creditsLabel;
        [SerializeField] ShopManager shopManager;

        void OnEnable()
        {
            if (shopManager != null)
            {
                shopManager.OnCreditsChanged += HandleCreditsChanged;
            }

            RefreshCredits();
        }

        void OnDisable()
        {
            if (shopManager != null)
            {
                shopManager.OnCreditsChanged -= HandleCreditsChanged;
            }
        }

        void HandleCreditsChanged(int _)
        {
            RefreshCredits();
        }

        void RefreshCredits()
        {
            GameManager manager = GameManager.inst;

            if (creditsLabel == null || manager == null || manager.Data == null)
            {
                return;
            }

            creditsLabel.text = manager.Data.credits.ToString();
        }
    }
}
