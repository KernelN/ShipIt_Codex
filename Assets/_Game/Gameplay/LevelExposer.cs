using UnityEngine;

namespace ShipIt.Gameplay
{
    public class LevelExposer : MonoBehaviour
    {
        [SerializeField] GameplayManager gameplayManager;
        [SerializeField] int levelIndex;

        void OnEnable()
        {
            if (!gameplayManager)
            {
                gameplayManager = GameplayManager.inst;
            }

            if (gameplayManager != null)
            {
                gameplayManager.OnOrderCompleted += HandleOrderCompleted;
            }
        }

        void OnDisable()
        {
            if (gameplayManager != null)
            {
                gameplayManager.OnOrderCompleted -= HandleOrderCompleted;
            }
        }

        void HandleOrderCompleted()
        {
            LevelManager.TryRegisterCompletedLevel(levelIndex);
        }
    }
}
