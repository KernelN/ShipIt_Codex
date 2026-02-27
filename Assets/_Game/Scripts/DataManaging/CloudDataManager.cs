using System.Collections.Generic;
using System.Threading.Tasks;
using ShipIt;
using Unity.Services.Authentication;
using Unity.Services.CloudSave;
using Unity.Services.CloudSave.Models;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.Events;

namespace Universal.FileManaging.Cloud
{
    public class CloudDataManager : Singleton<CloudDataManager>
    {
        internal override bool DoNotDestroyOnLoad => true;
        
        public enum Key { DeviceID, GameData, IdleData, _count }
        Dictionary<Key, object> data = new Dictionary<Key, object>();
        Dictionary<string, Item> loadedData;


        public bool hasDataConflict { get; private set; }
        public UnityEvent OnDataConflictFixed;
        public UnityEvent<Key, Item> OnDataLoaded;

        internal override void Awake()
        {
            base.Awake();
            
            if(this != inst) return;
            
            data = new Dictionary<Key, object>();
            loadedData = new Dictionary<string, Item>();
            
            SetupAndSignIn();
        }

        async Task SetupAndSignIn()
        {
            await UnityServices.InitializeAsync();
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            await LoadDataWithErrorHandling();
        }

        public void SaveKeyData(Key key, string data)
        {
            if(!this.data.TryAdd(key, data))
                this.data[key] = data;
        }
        
        public async Task SaveDataWithErrorHandling()
        {
            var data = new Dictionary<string, object>();
            foreach (var item in this.data) 
                data.Add(item.Key.ToString(), item.Value);
            try
            {
                Debug.Log("Attempting to save data...");
                await CloudSaveService.Instance.Data.Player.SaveAsync(data);
                Debug.Log("Save data success!");
            }
            catch (ServicesInitializationException e)
            {
                // service not initialized
                Debug.LogError(e);
            }
            catch (CloudSaveValidationException e)
            {
                // validation error
                Debug.LogError(e);
            }
            catch (CloudSaveRateLimitedException e)
            {
                // rate limited
                Debug.LogError(e);
            }
            catch (CloudSaveException e)
            {
                Debug.LogError(e);
            }
        }

        async Task LoadDataWithErrorHandling()
        {
            try
            {
                loadedData = await CloudSaveService.Instance.Data.Player.LoadAllAsync();
                
                if (loadedData.TryGetValue(nameof(Key.DeviceID), out var deviceIdItem))
                {
                    string deviceID = deviceIdItem.Value.GetAsString();
                    
                    if (deviceID != SystemInfo.deviceUniqueIdentifier)
                    {
                        hasDataConflict = true;
                        return;
                    }
                }
                else SaveKeyData(Key.DeviceID, SystemInfo.deviceUniqueIdentifier);
                
                for (int i = 0; i < (int)Key._count; i++)
                    if(loadedData.TryGetValue(((Key)i).ToString(), out var value))
                        OnDataLoaded?.Invoke((Key)i, value);
            }
            catch (ServicesInitializationException e)
            {
                // service not initialized
                Debug.LogError(e);
            }
            catch (CloudSaveValidationException e)
            {
                // validation error
                Debug.LogError(e);
            }
            catch (CloudSaveRateLimitedException e)
            {
                // rate limited
                Debug.LogError(e);
            }
            catch (CloudSaveException e)
            {
                Debug.LogError(e);
            }
        }

        public void ClearSaveData()
        {
            loadedData.Clear();
            CloudSaveService.Instance.Data.Player.DeleteAllAsync();
            hasDataConflict = false;
            OnDataConflictFixed?.Invoke();
        }

        public void KeepSaveData()
        {
            for (int i = 0; i < (int)Key._count; i++)
                if(loadedData.TryGetValue(((Key)i).ToString(), out var value))
                    OnDataLoaded?.Invoke((Key)i, value);
            hasDataConflict = false;
            OnDataConflictFixed?.Invoke();
        }

        public Item GetData(Key gameData) => loadedData.GetValueOrDefault(gameData.ToString());
    }
}