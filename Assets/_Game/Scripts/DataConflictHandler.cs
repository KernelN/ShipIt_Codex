using Unity.Services.CloudSave.Models;
using UnityEngine;
using Universal.SaveData;

namespace ShipIt.CloudData
{
    public class DataConflictHandler : MonoBehaviour
    {
        [SerializeField] GameDataShower localShower;
        [SerializeField] GameDataShower cloudShower;
        [SerializeField] GameObject dataConflictPanel;

        public void Start()
        {
            if(!CloudDataManager.inst.hasDataConflict) return;
            
            dataConflictPanel.SetActive(true);
            localShower?.Show(GameManager.inst.Data);
            Item cloudData = CloudDataManager.inst.GetData(CloudDataManager.Key.GameData);
            cloudShower?.Show(cloudData.Value.GetAs<GameData>());
        }
    }
}
