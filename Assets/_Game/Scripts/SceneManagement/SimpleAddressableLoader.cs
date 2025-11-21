using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class SimpleAddressableLoader : MonoBehaviour
{
    [SerializeField] List<AssetReferenceGameObject> addressables;
    List<GameObject> instances;

    void Awake()
    {
        instances = new List<GameObject>();
        for (int i = 0; i < addressables.Count; i++)
            addressables[i].LoadAssetAsync().Completed += OnAddresableLoaded;
    }
    void OnDestroy()
    {
        for (int i = 0; i < instances.Count; i++)
            Addressables.ReleaseInstance(instances[i]);
    }
    void OnAddresableLoaded(AsyncOperationHandle<GameObject> handle)
    {
        if (handle.Status == AsyncOperationStatus.Succeeded) 
            instances.Add(handle.Result);
    }
}   