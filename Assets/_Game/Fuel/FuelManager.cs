using UnityEngine;

namespace ShipIt
{
    public class FuelManager : MonoBehaviour
    {
        const string FuelResourceKey = "FuelTimerRatio";

        [SerializeField] int fuelPerRecharge = 1;
        [SerializeField] float rechargeMinutes = 5f;
        float timer;
        [SerializeField] int startingFuel = 10;
        [SerializeField] int maxFuel = 20;

        int currentFuel;
        GameManager gameManager;
        IdleManager idleManager;

        public System.Action<int> OnFuelChanged;
        
        public int CurrentFuel => currentFuel;
        float RechargeTime => rechargeMinutes * 60f;

        void Awake()
        {
            gameManager = GameManager.inst;
            idleManager = IdleManager.inst;

            InitializeFuel();

            if (idleManager) 
                idleManager.OnIdleEvaluated += HandleIdleEvaluation;
        }

        void OnDestroy()
        {
            if (idleManager != null)
            {
                idleManager.OnIdleEvaluated -= HandleIdleEvaluation;
                float ratio = RechargeTime > 0f ? Mathf.Clamp01(timer / RechargeTime) : 0f;
                idleManager.SaveResource(FuelResourceKey, ratio);
            }
        }

        void Update()
        {
            if (currentFuel >= maxFuel || RechargeTime <= 0f)
            {
                return;
            }

            timer += Time.deltaTime;

            if (timer >= RechargeTime)
            {
                timer -= RechargeTime;
                RechargeFuel(fuelPerRecharge);
            }
        }

        void InitializeFuel()
        {
            if (gameManager == null)
            {
                return;
            }

            GameData data = gameManager.Data;
            if (data.fuel < 0)
            {
                currentFuel = Mathf.Clamp(startingFuel, 0, maxFuel);
                data.fuel = currentFuel;
                gameManager.SaveGameData();
            }
            else
            {
                currentFuel = Mathf.Clamp(data.fuel, 0, maxFuel);
            }

            OnFuelChanged?.Invoke(currentFuel);
        }
        void HandleIdleEvaluation()
        {
            if (!idleManager || RechargeTime <= 0f) return;

            float savedRatio = idleManager.GetResource(FuelResourceKey);
            float savedSeconds = savedRatio * RechargeTime;
            float elapsedSeconds = idleManager.HasLastCheck ? (float)idleManager.OfflineTimeSpan.TotalSeconds : 0f;
            float totalSeconds = savedSeconds + elapsedSeconds;

            if (totalSeconds <= 0f) return;

            int cycles = Mathf.FloorToInt(totalSeconds / RechargeTime);
            float remainder = totalSeconds % RechargeTime;

            if (cycles > 0) 
                RechargeFuel(cycles * fuelPerRecharge);

            timer = remainder;
            idleManager.SaveResource(FuelResourceKey, RechargeTime > 0f ? Mathf.Clamp01(timer / RechargeTime) : 0f);
        }
        void RechargeFuel(int amount)
        {
            if (amount <= 0) return;

            int previousFuel = currentFuel;
            currentFuel = Mathf.Min(currentFuel + amount, maxFuel);

            if (currentFuel != previousFuel)
            {
                PersistFuel();
                OnFuelChanged?.Invoke(currentFuel);
            }
        }
        void PersistFuel()
        {
            if (!gameManager) return;

            GameData data = gameManager.Data;
            data.fuel = currentFuel;
            gameManager.SaveGameData();
        }
    }
}