using UnityEngine;
using UnityEngine.Serialization;
using Universal;
using ShipIt.TickManaging;

namespace ShipIt.Gameplay.Astral
{
    public class OrderManager : Singleton<OrderManager>
    {
        [SerializeField] float orderCredits;
        [SerializeField] float tipCredits;
        [FormerlySerializedAs("orderTime")]
        [SerializeField] float planningTime;
        [SerializeField] float movingTime;
        [SerializeField] float movingBonusTime;
        float currentOrderCredits;
        float currentTipCredits;
        float tipTimer;
        float orderTimer;
        float totalOrderTime;
        float totalTipTime;
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

        internal override void OnDestroy()
        {
            UnsubscribeFromTicks();
            base.OnDestroy();
        }

        void InitializeCredits()
        {
            totalOrderTime = planningTime + movingTime;
            totalTipTime = planningTime + movingBonusTime;

            currentOrderCredits = orderCredits;
            currentTipCredits = tipCredits;
            tipTimer = totalTipTime;
            orderTimer = totalOrderTime;
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
            return UpdateCreditsOverTime(ref currentTipCredits, tipCredits, ref tipTimer, totalTipTime);
        }

        bool UpdateOrderCredits()
        {
            return UpdateCreditsOverTime(ref currentOrderCredits, orderCredits, ref orderTimer, totalOrderTime);
        }

        bool UpdateCreditsOverTime(ref float currentCredits, float maxCredits, ref float timer, float totalTime)
        {
            if (maxCredits <= 0f || totalTime <= 0f)
            {
                bool hadCredits = currentCredits > 0f;
                currentCredits = 0f;
                timer = 0f;
                return hadCredits;
            }

            if (timer <= 0f)
            {
                bool hadCredits = currentCredits > 0f;
                currentCredits = 0f;
                return hadCredits;
            }

            float previousCredits = currentCredits;
            timer = Mathf.Max(0f, timer - TickInterval);
            currentCredits = maxCredits * timer / totalTime;

            return !Mathf.Approximately(previousCredits, currentCredits);
        }

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
