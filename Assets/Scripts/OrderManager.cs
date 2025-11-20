using System;
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

        const float TickInterval = 0.1f;

        float currentOrderCredits;
        float currentTipCredits;
        float tipTimer;
        float orderTimer;
        bool tickSubscribed;

        public float CurrentOrderCredits => currentOrderCredits;
        public float CurrentTipCredits => currentTipCredits;
        public float TotalCredits => currentOrderCredits + currentTipCredits;
        public float OrderRemainingRatio => orderCredits > 0 ? Mathf.Clamp01(currentOrderCredits / orderCredits) : 0f;
        public float TipRemainingRatio => tipCredits > 0 ? Mathf.Clamp01(currentTipCredits / tipCredits) : 0f;

        public event Action<float, float> CreditsUpdated;
        public event Action TargetReached;

        internal override bool DoNotDestroyOnLoad => false;

        internal override void Awake()
        {
            base.Awake();

            if (inst != this)
            {
                return;
            }

            InitializeCredits();
            SubscribeToTicks();
        }

        void Start()
        {
            if (inst != this)
            {
                return;
            }

            SubscribeToTicks();
            RaiseCreditsUpdated();
        }

        void OnDisable() => UnsubscribeFromTicks();

        public void OnTargetReached()
        {
            UnsubscribeFromTicks();
            TargetReached?.Invoke();
        }

        void OnDestroy() => UnsubscribeFromTicks();

        void InitializeCredits()
        {
            currentOrderCredits = Mathf.Max(0f, orderCredits);
            currentTipCredits = Mathf.Max(0f, tipCredits);
            tipTimer = 0f;
            orderTimer = 0f;
        }

        void SubscribeToTicks()
        {
            if (tickSubscribed || UpdateManager.inst == null)
            {
                return;
            }

            UpdateManager.inst.SuscribeToScaled(TickInterval, TickCredits);
            tickSubscribed = true;
        }

        void UnsubscribeFromTicks()
        {
            if (!tickSubscribed || UpdateManager.inst == null)
            {
                return;
            }

            UpdateManager.inst.RemoveFromScaled(TickInterval, TickCredits);
            tickSubscribed = false;
        }

        void TickCredits()
        {
            if (inst != this)
            {
                return;
            }

            bool creditsChanged = false;

            creditsChanged |= UpdateTipCredits();
            creditsChanged |= UpdateOrderCredits();

            if (creditsChanged)
            {
                RaiseCreditsUpdated();
            }
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

            if (tipTimer >= tipTime)
            {
                return false;
            }

            float previousCredits = currentTipCredits;
            tipTimer = Mathf.Min(tipTimer + TickInterval, tipTime);

            float remainingRatio = 1f - (tipTimer / tipTime);
            currentTipCredits = Mathf.Max(0f, tipCredits * remainingRatio);

            return !Mathf.Approximately(previousCredits, currentTipCredits);
        }

        bool UpdateOrderCredits()
        {
            if (!HasTipEnded())
            {
                return false;
            }

            if (orderCredits <= 0f || orderTime <= 0f)
            {
                bool orderWasAvailable = currentOrderCredits > 0f;
                currentOrderCredits = 0f;
                orderTimer = orderTime;
                return orderWasAvailable;
            }

            if (orderTimer >= orderTime)
            {
                return false;
            }

            float previousCredits = currentOrderCredits;
            orderTimer = Mathf.Min(orderTimer + TickInterval, orderTime);

            float remainingRatio = 1f - (orderTimer / orderTime);
            currentOrderCredits = Mathf.Max(0f, orderCredits * remainingRatio);

            return !Mathf.Approximately(previousCredits, currentOrderCredits);
        }

        bool HasTipEnded() => tipTimer >= tipTime || tipCredits <= 0f || tipTime <= 0f;

        void RaiseCreditsUpdated()
        {
            CreditsUpdated?.Invoke(currentOrderCredits, currentTipCredits);
        }
    }
}
