using UnityEngine;

namespace ShipIt.Plugins.DebugTools
{
    public class LogPrinter : MonoBehaviour
    {
        //[Header("Set Values")]
        [SerializeField] TMPro.TextMeshProUGUI text;
        //[Header("Runtime Values")]

        //Unity Events
        void Awake()
        {
            Application.logMessageReceived += HandleUnityLog;
        }
        void OnDestroy()
        {
            Application.logMessageReceived -= HandleUnityLog;
        }

        //Methods
        public void Clear()
        {
            text.text = "";
        }
        void HandleUnityLog(string logString, string stacktrace, LogType type)
        {
            text.text += logString + "\n \n";
        }
    }
}
