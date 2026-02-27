using System;
using UnityEngine;

namespace Universal.FileManaging.Cloud
{
    public class CloudSaveTriggerer : MonoBehaviour
    {
        CloudDataManager dataManager;
        void Start()
        {
            dataManager = CloudDataManager.inst;
            if (!dataManager)
            {
                Destroy(this);
                return;
            }
            
            if(dataManager.hasDataConflict)
                dataManager.OnDataConflictFixed.AddListener(OnDataConflictSolved);
            else
                dataManager.SaveDataWithErrorHandling();
        }
        
        public void OnDataConflictSolved() => dataManager.SaveDataWithErrorHandling();
    }
}
