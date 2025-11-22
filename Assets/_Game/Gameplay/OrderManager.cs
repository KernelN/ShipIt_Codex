using UnityEngine;
using Universal;
using ShipIt.TickManaging;

namespace ShipIt.Gameplay.Astral
{
    public class OrderManager : Singleton<OrderManager>
    {
        [SerializeField] float orderCredits;
        [SerializeField] float tipCredits;
        [SerializeField] float orderTime;
        [SerializeField] float tipTime;
        float currentOrderCredits;
        float currentTipCredits;
        float tipTimer;
        float orderTimer;
        bool tickSubscribed;
        bool creditsDepleted;

        const float TickInterval = 0.1f;

        public float CurrentOrderCredits => currentOrderCredits;
        public float CurrentTipCredits => currentTipCredits;
        public float TotalCredits => currentOrderCredits + currentTipCredits;
        public float OrderRemainingRatio => orderCredits > 0 ? Mathf.Clamp01(currentOrderCredits / orderCredits) : 0f;
        public float TipRemainingRatio => tipCredits > 0 ? Mathf.Clamp01(currentTipCredits / tipCredits) : 0f;

        public System.Action<float, float> CreditsUpdated;
        public System.Action TargetReached;
        public System.Action CreditsDepleted;

        internal override void Awake()
        {
            base.Awake();

            if (inst != this)
            {
                return;
            }

            InitializeCredits();
        }
        void Start()
        {
            if (inst != this)
                return;

            SubscribeToTicks();
            RaiseCreditsUpdated();
        }

        void OnDisable() => UnsubscribeFromTicks();

        public void OnTargetReached()
        {
            UnsubscribeFromTicks();
            SaveOrderCredits();
            TargetReached?.Invoke();
        }

        void OnDestroy() => UnsubscribeFromTicks();

        void InitializeCredits()
        {
            currentOrderCredits = orderCredits;
            currentTipCredits = tipCredits;
            tipTimer = tipTime;
            orderTimer = orderTime;
        }

        void SubscribeToTicks()
        {
            if (tickSubscribed || !UpdateManager.inst)
                return;

            UpdateManager.inst.SuscribeToScaled(TickInterval, TickCredits);
            tickSubscribed = true;
        }

        void UnsubscribeFromTicks()
        {
            if (!tickSubscribed || !UpdateManager.inst)
                return;

            UpdateManager.inst.RemoveFromScaled(TickInterval, TickCredits);
            tickSubscribed = false;
        }

        void TickCredits()
        {
            if (inst != this)
                return;

            bool creditsChanged = false;

            creditsChanged |= UpdateTipCredits();
            creditsChanged |= UpdateOrderCredits();

            if (creditsChanged) RaiseCreditsUpdated();
        }

        bool UpdateTipCredits()
        {
            if (tipCredits <= 0f || tipTime <= 0f)
            {
                bool tipWasAvailable = currentTipCredits > 0f;
                currentTipCredits = 0f;
                tipTimer = tipTime;
                return tipWasAvailable;
            }

            if (tipTimer <= 0)
            {
                currentTipCredits = 0;
                return false;
            }

            tipTimer -= TickInterval;

            currentTipCredits = tipCredits * tipTimer / tipTime;

            return true;
        }

        bool UpdateOrderCredits()
        {
            if (!HasTipEnded()) return false;

            if (orderCredits <= 0f || orderTime <= 0f)
            {
                bool orderWasAvailable = currentOrderCredits > 0f;
                currentOrderCredits = 0f;
                orderTimer = orderTime;
                return orderWasAvailable;
            }

            if (orderTimer <= 0)
            {
                currentOrderCredits = 0;
                return false;
            }

            orderTimer -= TickInterval;

            currentOrderCredits = orderCredits * orderTimer / orderTime;

            return true;
        }

        bool HasTipEnded() => tipTimer <= 0 || tipCredits <= 0f;

        void RaiseCreditsUpdated()
        {
            if (TotalCredits <= 0f)
            {
                currentOrderCredits = 0f;
                currentTipCredits = 0f;

                CreditsUpdated?.Invoke(currentOrderCredits, currentTipCredits);

                if (!creditsDepleted)
                {
                    creditsDepleted = true;
                    CreditsDepleted?.Invoke();
                }

                return;
            }

            CreditsUpdated?.Invoke(currentOrderCredits, currentTipCredits);
        }

        void SaveOrderCredits()
        {
            GameManager manager = GameManager.inst;
            if (manager?.Data == null)
            {
                return;
            }

            manager.Data.credits += Mathf.RoundToInt(TotalCredits);
            manager.SaveGameData();
        }
    }
}
