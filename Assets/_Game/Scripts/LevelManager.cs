using UnityEngine;

namespace ShipIt
{
    public static class LevelManager
    {
        public static int HighestCompletedLevel
        {
            get
            {
                GameManager gameManager = GameManager.inst;
                if (gameManager == null || gameManager.Data == null)
                {
                    return -1;
                }

                return gameManager.Data.highestLevelCompleted;
            }
        }

        public static bool TryRegisterCompletedLevel(int levelIndex)
        {
            if (levelIndex < 0)
            {
                return false;
            }

            GameManager gameManager = GameManager.inst;
            if (gameManager == null)
            {
                return false;
            }

            GameData data = gameManager.Data;
            if (data == null)
            {
                return false;
            }

            if (levelIndex <= data.highestLevelCompleted)
            {
                return false;
            }

            data.highestLevelCompleted = levelIndex;
            gameManager.SaveGameData();
            return true;
        }
    }
}
