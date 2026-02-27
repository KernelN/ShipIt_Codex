using UnityEngine;
using UnityEngine.Advertisements;

namespace Universal.Ads
{
    public class AdsInitializer : MonoBehaviour, IUnityAdsInitializationListener
    {
        [SerializeField] string androidGameId;
        [SerializeField] string iosGameId;
        [SerializeField] bool isTesting;
        [SerializeField] AdController[] adsInScene;

        string gameId;


        void Awake()
        {
#if UNITY_IOS
                gameId = iosGameId;
#elif UNITY_ANDROID
                gameId = androidGameId;
#elif UNITY_EDITOR
            gameId = androidGameId; // Use android by default
#endif

            if (!Advertisement.isInitialized && Advertisement.isSupported)
            {
                Advertisement.Initialize(gameId, isTesting, this);
            }

            for (int i = 0; i < adsInScene.Length; i++) 
                adsInScene[i].Initialize();
        }


        public void OnInitializationComplete()
        {
            Debug.Log("Ads Initialized...");
        }

        public void OnInitializationFailed(UnityAdsInitializationError error, string message)
        {
        }
    }
}