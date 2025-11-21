using System;
using UnityEngine;

namespace ShipIt.Plugins
{
    public class UILoggerPlugin : MonoBehaviour
    {
        //[Header("Set Values")]
        [SerializeField] TMPro.TextMeshProUGUI label;
        //[Header("Runtime Values")]
        LoggerPlugin logger;

        //Unity Events
        void Start()
        {
            logger = LoggerPlugin.inst;
            if (!logger)
            {
                Destroy(this);
                return;
            }
            
            label.text = "Start";
        }

        //Methods
        
        public void RegisterTimeLog() => logger.RegisterTimeLog();
        public void RegisterLog(string log) => logger.RegisterLog(log); 
        public void GetLogs() => label.text = logger.GetLogs(); 
        public void ReadLogs()
        {
            logger.ReadLogs();
            GetLogs();
        }

        public void ClearLogs() => logger.ClearLogs(); 
    }
}
