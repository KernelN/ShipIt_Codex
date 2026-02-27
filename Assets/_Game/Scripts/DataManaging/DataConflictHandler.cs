using Unity.Services.CloudSave.Models;
using UnityEngine;
using Universal.FileManaging.Cloud;

namespace ShipIt.CloudData
{
    public class DataConflictHandler : MonoBehaviour
    {
        [SerializeField] GameDataShower localShower;
        [SerializeField] GameDataShower cloudShower;
        [SerializeField] GameObject dataConflictPanel;

        public void Start()
        {
            CloudDataManager dataManager = CloudDataManager.inst;
            if(!dataManager) return;
            if(!dataManager.hasDataConflict) return;
            
            dataConflictPanel.SetActive(true);
            localShower?.Show(GameManager.inst.Data);
            Item cloudData = dataManager.GetData(CloudDataManager.Key.GameData);
            cloudShower?.Show(cloudData.Value.GetAs<GameData>());
        }
    }
}
