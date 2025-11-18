using UnityEngine;
using Universal;

namespace ShipIt.Plugins
{
    public class LoggerPlugin : Singleton<LoggerPlugin>
    {
        //Plugin:
        //Con un boton muestra los logs
        //Con un boton registra los logs
        //Con otro los borra
        
        //[Header("Set Values")]
        //[Header("Runtime Values")]

        const string pluginPackName = "com.insausti.loggerplugin";
        const string pluginClassName = pluginPackName + ".Logger";

#if UNITY_ANDROID || PLATFORM_ANDROID
        AndroidJavaClass pluginClass;
        AndroidJavaObject pluginInst;
#endif
        
        //Unity Events
        internal override void Awake()
        {
            if (Application.isEditor)
            {
                Destroy(this);
                return;
            }
            
            base.Awake();

#if UNITY_ANDROID || PLATFORM_ANDROID
            pluginClass = new AndroidJavaClass(pluginClassName);
            pluginInst = pluginClass.CallStatic<AndroidJavaObject>("getInstance");
            
            AndroidJavaClass unityClass =
                new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject activity = 
                unityClass.GetStatic<AndroidJavaObject>("currentActivity");
            pluginInst.Call("Set", activity);
#endif
            
            Application.logMessageReceived += HandleUnityLog;
        }
        void OnDestroy()
        {
            Application.logMessageReceived -= HandleUnityLog;
        }

        //Methods
        public void RegisterTimeLog()
        {
            Debug.Log("Unity - Send Log: " + Time.time);
        }
        public void RegisterLog(string log)
        {
#if UNITY_ANDROID || PLATFORM_ANDROID
            pluginInst.Call("SendLog", log);
#endif
        }
        public string GetLogs()
        {
#if UNITY_ANDROID || PLATFORM_ANDROID
            if (pluginInst != null)
                return pluginInst.Call<string>("GetLogs");
            else
                return "Plugin not found";
#endif
            return "Plugin not available";
        }
        public void ReadLogs()
        {
#if UNITY_ANDROID || PLATFORM_ANDROID
            pluginInst.Call("ReadLogs");
#endif
        }
        public void ClearLogs()
        {
#if UNITY_ANDROID || PLATFORM_ANDROID
            pluginInst.Call("ClearLogs");
#endif
        }
        
        //Event Receivers
        void HandleUnityLog(string logString, string stacktrace, LogType type)
        {
            RegisterLog(logString);
        }
    }
}