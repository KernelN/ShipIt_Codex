using System.Collections.Generic;
using UnityEngine;
using Universal.FileManaging;

namespace ShipIt
{
    [System.Serializable]
    public class IdleResource
    {
        public string name;
        public float ratio;
    }

    [System.Serializable]
    public class IdleData
    {
        public System.DateTime lastCheckTime;
        public List<IdleResource> resources = new List<IdleResource>();
    }

    public class IdleManager : Universal.Singleton<IdleManager>
    {
        const string DataPath = "/Data/IdleData.dat";

        IdleData data;

        public System.Action OnIdleEvaluated;
        public System.TimeSpan OfflineTimeSpan { get; private set; }
        public bool HasLastCheck { get; private set; }

        internal override void Awake()
        {
            base.Awake();
            if (this != inst)
            {
                return;
            }

            LoadIdleData();

            if (data == null)
                data = new IdleData();
            else if (data.resources == null) 
                data.resources = new List<IdleResource>();
        }

        void Start()
        {
            EvaluateLastCheck();
        }

        void EvaluateLastCheck()
        {
            HasLastCheck = data.lastCheckTime != default;
            if (HasLastCheck)
                OfflineTimeSpan = System.DateTime.Now - data.lastCheckTime;
            else
                OfflineTimeSpan = System.TimeSpan.Zero;
            OnIdleEvaluated?.Invoke();
        }

        public void SaveResource(string name, float ratio)
        {
            if (data == null)
            {
                data = new IdleData();
            }

            IdleResource resource = data.resources.Find(r => r.name == name);
            if (resource == null)
            {
                data.resources.Add(new IdleResource { name = name, ratio = ratio });
            }
            else
            {
                resource.ratio = ratio;
            }
        }

        public float GetResource(string name)
        {
            IdleResource resource = data?.resources?.Find(r => r.name == name);
            return resource?.ratio ?? 0f;
        }

        internal override void OnDestroy()
        {
            if (inst == this)
            {
                if (data == null)
                {
                    data = new IdleData();
                }

                data.lastCheckTime = System.DateTime.Now;
                SaveIdleData();
            }

            base.OnDestroy();
        }

        void LoadIdleData()
        {
            string path = Application.persistentDataPath + DataPath;
            data = FileManager<IdleData>.LoadDataFromFile(path);
        }

        void SaveIdleData()
        {
            string path = Application.persistentDataPath + DataPath;
            FileManager<IdleData>.SaveDataToFile(data, path);
        }
    }
}
