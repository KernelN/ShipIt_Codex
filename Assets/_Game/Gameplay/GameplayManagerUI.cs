using UnityEngine;

namespace ShipIt.Gameplay
{
    public class GameplayManagerUI : MonoBehaviour
    {
        [SerializeField] GameplayManager gameplayManager;
        [SerializeField] GameObject orderCompletedIndicator;
        [SerializeField] GameObject orderFailedIndicator;

        void OnEnable()
        {
            if (!gameplayManager)
            {
                gameplayManager = GameplayManager.inst;
            }

            Subscribe();
            RefreshIndicators();
        }

        void OnDisable()
        {
            Unsubscribe();
        }

        void Subscribe()
        {
            if (gameplayManager == null)
            {
                return;
            }

            gameplayManager.OnOrderCompleted += ShowOrderCompleted;
            gameplayManager.OnOrderFailed += ShowOrderFailed;
        }

        void Unsubscribe()
        {
            if (gameplayManager == null)
            {
                return;
            }

            gameplayManager.OnOrderCompleted -= ShowOrderCompleted;
            gameplayManager.OnOrderFailed -= ShowOrderFailed;
        }

        void RefreshIndicators()
        {
            if (gameplayManager == null)
            {
                SetIndicator(orderCompletedIndicator, false);
                SetIndicator(orderFailedIndicator, false);
                return;
            }

            SetIndicator(orderCompletedIndicator, gameplayManager.OrderCompleted);
            SetIndicator(orderFailedIndicator, gameplayManager.OrderFailed);
        }

        void ShowOrderCompleted() => SetIndicator(orderCompletedIndicator, true);
        void ShowOrderFailed() => SetIndicator(orderFailedIndicator, true);

        static void SetIndicator(GameObject indicator, bool shouldEnable)
        {
            if (indicator != null)
            {
                indicator.SetActive(shouldEnable);
            }
        }
    }
}
