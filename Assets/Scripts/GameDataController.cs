using ShipIt;
using UnityEngine;
using Universal.FileManaging;

namespace ShipIt.DataManagement
{
    public class GameDataController : Universal.Singleton<GameDataController>
    {
        const string DataPath = "/Data/GameData.dat";

        GameData data;
        public GameData Data => data;

        internal override void Awake()
        {
            base.Awake();
            if (this != inst) return;

            Load();

            if (data == null)
            {
                data = new GameData();
            }
        }

        public void Save()
        {
            string path = Application.persistentDataPath + DataPath;
            FileManager<GameData>.SaveDataToFile(data, path);
        }

        public void Load()
        {
            string path = Application.persistentDataPath + DataPath;
            data = FileManager<GameData>.LoadDataFromFile(path);
        }
    }
}
