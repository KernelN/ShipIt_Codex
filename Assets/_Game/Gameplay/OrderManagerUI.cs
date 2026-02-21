using TMPro;
using UnityEngine;

namespace ShipIt.Gameplay.Astral
{
    public class OrderManagerUI : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI creditsLabel;
        [SerializeField] TextMeshProUGUI paymentLabel;
        [SerializeField] TextMeshProUGUI tipLabel;
        [SerializeField] Color tipStartColor = Color.white;
        [SerializeField] Color tipEndColor = Color.green;
        [SerializeField] Color orderStartColor = Color.white;
        [SerializeField] Color orderEndColor = Color.red;

        OrderManager orderManager;

        void Start()
        {
            orderManager = OrderManager.inst;
            if (orderManager)
            {
                orderManager.CreditsUpdated += OnCreditsUpdated;
                OnCreditsUpdated(orderManager.CurrentOrderCredits, orderManager.CurrentTipCredits);
            }
        }
        void OnDestroy()
        {
            if (orderManager != null) 
                orderManager.CreditsUpdated -= OnCreditsUpdated;
        }

        void OnCreditsUpdated(float orderCredits, float tipCredits)
        {
            float totalCredits = orderCredits + tipCredits;
            if (creditsLabel)
                creditsLabel.text = totalCredits.ToString("$0");
            
            float orderRatio;
            
            if (paymentLabel)
            {
                paymentLabel.text = orderCredits.ToString("$0");
                orderRatio = orderManager.OrderRemainingRatio;
                paymentLabel.color = Color.Lerp(orderEndColor, orderStartColor, orderRatio);
            }

            if (tipLabel)
            {
                tipLabel.text = tipCredits.ToString("$0");
                float tipRatio = orderManager.TipRemainingRatio;
                tipLabel.color = Color.Lerp(tipEndColor, tipStartColor, tipRatio);
            }

            if (!creditsLabel)
                return;

            if (tipCredits > 0f)
            {
                float tipRatio = orderManager.TipRemainingRatio;
                creditsLabel.color = Color.Lerp(tipEndColor, tipStartColor, tipRatio);
                return;
            }

            orderRatio = orderManager.OrderRemainingRatio;
            creditsLabel.color = Color.Lerp(orderEndColor, orderStartColor, orderRatio);
        }
    }
}
