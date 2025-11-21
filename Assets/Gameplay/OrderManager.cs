using UnityEngine;
using Universal;
using ShipIt.TickManaging;
using ShipIt.Gameplay;

namespace ShipIt.Gameplay.Astral
{
    public class OrderManager : Singleton<OrderManager>
    {
        internal override bool DoNotDestroyOnLoad => false;

        [SerializeField] float orderCredits;
        [SerializeField] float tipCredits;
        [SerializeField] float orderTime;
        [SerializeField] float tipTime;
        [Header("Damage Penalty")]
        [SerializeField, Min(0f)] float damageTimePenalty = 1f;
        [SerializeField] Ship ship;
        float currentOrderCredits;
        float currentTipCredits;
        float tipTimer;
        float orderTimer;
        bool tickSubscribed;
        bool shipDamageSubscribed;

        const float TickInterval = 0.1f;

        public float CurrentOrderCredits => currentOrderCredits;
        public float CurrentTipCredits => currentTipCredits;
        public float TotalCredits => currentOrderCredits + currentTipCredits;
        public float OrderRemainingRatio => orderCredits > 0 ? Mathf.Clamp01(currentOrderCredits / orderCredits) : 0f;
        public float TipRemainingRatio => tipCredits > 0 ? Mathf.Clamp01(currentTipCredits / tipCredits) : 0f;

        public System.Action<float, float> CreditsUpdated;
        public System.Action TargetReached;

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

        void OnEnable()
        {
            if (inst != this)
            {
                return;
            }

            SubscribeToShipDamage();
        }

        void OnDisable()
        {
            UnsubscribeFromTicks();
            UnsubscribeFromShipDamage();
        }

        public void OnTargetReached()
        {
            UnsubscribeFromTicks();
            SaveOrderCredits();
            TargetReached?.Invoke();
        }

        void OnDestroy()
        {
            UnsubscribeFromTicks();
            UnsubscribeFromShipDamage();
        }

        void InitializeCredits()
        {
            currentOrderCredits = orderCredits;
            currentTipCredits = tipCredits;
            tipTimer = 0f;
            orderTimer = 0f;
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

        void SubscribeToShipDamage()
        {
            if (shipDamageSubscribed)
                return;

            if (!ship)
            {
                ship = FindObjectOfType<Ship>();
            }

            if (!ship)
                return;

            ship.Damaged += OnShipDamaged;
            shipDamageSubscribed = true;
        }

        void UnsubscribeFromShipDamage()
        {
            if (!shipDamageSubscribed || !ship)
                return;

            ship.Damaged -= OnShipDamaged;
            shipDamageSubscribed = false;
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

        void OnShipDamaged()
        {
            if (inst != this)
            {
                return;
            }

            if (!HasTipEnded())
            {
                tipTimer += damageTimePenalty;
            }
            else
            {
                orderTimer += damageTimePenalty;
            }

            if (!tickSubscribed)
            {
                SubscribeToTicks();
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

            if (tipTimer <= 0)
                return false;

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
                return false;

            orderTimer -= TickInterval;

            currentOrderCredits = orderCredits * orderTimer / orderTime;

            return true;
        }

        bool HasTipEnded() => tipTimer <= 0 || tipCredits <= 0f;

        void RaiseCreditsUpdated()
        {
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
