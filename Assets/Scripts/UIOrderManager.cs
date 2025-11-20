using TMPro;
using UnityEngine;
using ShipIt.Gameplay.Astral;

namespace ShipIt.Gameplay.Astral
{
    public class UIOrderManager : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI creditsLabel;
        [SerializeField] Color tipStartColor = Color.white;
        [SerializeField] Color tipEndColor = Color.green;
        [SerializeField] Color orderStartColor = Color.white;
        [SerializeField] Color orderEndColor = Color.red;
        [SerializeField] GameObject targetReachedIndicator;

        OrderManager orderManager;

        void OnEnable()
        {
            orderManager = OrderManager.inst;
            if (orderManager != null)
            {
                orderManager.CreditsUpdated += OnCreditsUpdated;
                orderManager.TargetReached += OnTargetReached;
                OnCreditsUpdated(orderManager.CurrentOrderCredits, orderManager.CurrentTipCredits);
                EnableTargetReachedIndicator(false);
            }
        }

        void OnDisable()
        {
            if (orderManager != null)
            {
                orderManager.CreditsUpdated -= OnCreditsUpdated;
                orderManager.TargetReached -= OnTargetReached;
            }
        }

        void OnCreditsUpdated(float orderCredits, float tipCredits)
        {
            if (creditsLabel == null)
            {
                return;
            }

            float totalCredits = orderCredits + tipCredits;
            creditsLabel.text = totalCredits.ToString("0.##");

            if (tipCredits > 0f)
            {
                float tipRatio = orderManager != null ? orderManager.TipRemainingRatio : 0f;
                creditsLabel.color = Color.Lerp(tipEndColor, tipStartColor, tipRatio);
                return;
            }

            float orderRatio = orderManager != null ? orderManager.OrderRemainingRatio : 0f;
            creditsLabel.color = Color.Lerp(orderEndColor, orderStartColor, orderRatio);
        }

        void OnTargetReached()
        {
            EnableTargetReachedIndicator();
        }

        void EnableTargetReachedIndicator(bool shouldEnable = true)
        {
            if (targetReachedIndicator != null)
            {
                targetReachedIndicator.SetActive(shouldEnable);
            }
        }
    }
}
