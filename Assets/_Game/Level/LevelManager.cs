namespace ShipIt
{
    public static class LevelManager
    {
        public static int HighestCompletedLevel
        {
            get
            {
                GameManager gameManager = GameManager.inst;
                if (!gameManager || gameManager.Data == null)
                {
                    return 0;
                }

                return gameManager.Data.highestLevelCompleted;
            }
        }

        public static bool TryRegisterCompletedLevel(int level)
        {
            if (level < 0)
                return false;

            GameManager gameManager = GameManager.inst;
            if (!gameManager)
                return false;

            GameData data = gameManager.Data;
            if (data == null)
                return false;

            if (level <= data.highestLevelCompleted)
                return false;

            data.highestLevelCompleted = level;
            gameManager.SaveGameData();
            return true;
        }
    }
}