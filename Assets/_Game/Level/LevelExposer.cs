using UnityEngine;

namespace ShipIt.Gameplay
{
    public class LevelExposer : MonoBehaviour
    {
        [SerializeField] GameplayManager gameplayManager;
        [SerializeField] int level;

        void OnEnable()
        {
            if (!gameplayManager) 
                gameplayManager = GameplayManager.inst;

            if (gameplayManager) 
                gameplayManager.OnOrderCompleted += HandleOrderCompleted;
        }

        void OnDisable()
        {
            if (gameplayManager) 
                gameplayManager.OnOrderCompleted -= HandleOrderCompleted;
        }

        void HandleOrderCompleted()
        {
            LevelManager.TryRegisterCompletedLevel(level);
        }
    }
}