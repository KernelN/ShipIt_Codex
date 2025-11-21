using UnityEngine;
using Universal.FileManaging;

namespace ShipIt
{
    public class GameManager : Universal.Singleton<GameManager>
    {
        internal override bool DoNotDestroyOnLoad => true;
        const string DataPath = "/Data/GameData.dat";

        [SerializeField] GameData data;
        public GameData Data => data;

        internal override void Awake()
        {
            base.Awake();

            if (this != inst) return;

            LoadGameData();

            if (data == null) 
                data = new GameData();
        }

        //Methods
        public void QuitGame()
        {
            SaveGameData();
            Application.Quit();
        }
        public void SaveGameData()
        {
            string path = Application.persistentDataPath + DataPath;
            FileManager<GameData>.SaveDataToFile(data, path);
        }
        public void LoadGameData()
        {
            string path = Application.persistentDataPath + DataPath;
            data = FileManager<GameData>.LoadDataFromFile(path);
        }
        [ContextMenu("Clear Game Data")]
        public void ClearGameData()
        {
            data = new GameData();
            SaveGameData();
        }
    }
}
