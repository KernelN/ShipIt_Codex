using System.Threading.Tasks;
using UnityEngine;
using Universal.FileManaging;
using CloudDataManager = Universal.FileManaging.Cloud.CloudDataManager;

namespace ShipIt
{
    public class GameManager : Universal.Singleton<GameManager>
    {
        internal override bool DoNotDestroyOnLoad => true;
        internal override bool DestroyGameObject => true;
        const string DataPath = "/Data/GameData.dat";

        [SerializeField] GameData data;
        public GameData Data => data;
        CloudDataManager cloudDataManager;

        internal override void Awake()
        {
            base.Awake();

            if (this != inst) return;

            LoadGameData();
            
            cloudDataManager = CloudDataManager.inst;
            cloudDataManager.OnDataLoaded.AddListener(OnCloudLoaded);
        }

        void OnCloudLoaded(CloudDataManager.Key key, Unity.Services.CloudSave.Models.Item data)
        {
            if(key != CloudDataManager.Key.GameData) return;
            
            string dataString = data.Value.GetAs<string>();
            this.data = JsonUtility.FromJson<GameData>(dataString);
            SaveGameData();
        }

        internal override void OnDestroy()
        {
            if(this != inst) return;
            
            SaveGameData();
            cloudDataManager.SaveDataWithErrorHandling();
            
            base.OnDestroy();
        }

        //Methods
        public void SaveGameData()
        {
            string path = Application.persistentDataPath + DataPath;
            FileManager<GameData>.SaveDataToFile(data, path);
            string dataString = JsonUtility.ToJson(data);
            cloudDataManager.SaveKeyData(CloudDataManager.Key.GameData, dataString);
        }
        public void LoadGameData()
        {
            string path = Application.persistentDataPath + DataPath;
            data = FileManager<GameData>.LoadDataFromFile(path);
            if(data == null) data = new GameData();
        }
        [ContextMenu("Clear Game Data")]
        public void ClearGameData()
        {
            data = new GameData();
            SaveGameData();
        }
    }
}
