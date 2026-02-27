using UnityEngine;
using UnityEngine.Advertisements;
using UnityEngine.Events;

namespace Universal.Ads
{
    public class AdController : MonoBehaviour, IUnityAdsLoadListener, IUnityAdsShowListener
    {
        [SerializeField] string androidAdUnitId;
        [SerializeField] string iosAdUnitId;

        string adUnitId;

        public UnityEvent OnAdCompleted;

        public void Initialize()
        {
#if UNITY_IOS
                adUnitId = iosAdUnitId;
#elif UNITY_ANDROID
                adUnitId = androidAdUnitId;
#endif

            LoadAd();
        }


        void LoadAd()
        {
            Advertisement.Load(adUnitId, this);
        }

        public void ShowAd()
        {
            Advertisement.Show(adUnitId, this);
            LoadAd();
        }




        #region LoadCallbacks

        public void OnUnityAdsAdLoaded(string placementId)
        {
            Debug.Log(adUnitId + " Ad Loaded");
        }

        public void OnUnityAdsFailedToLoad(string placementId, UnityAdsLoadError error, string message)
        {
        }

        #endregion

        #region ShowCallbacks

        public void OnUnityAdsShowFailure(string placementId, UnityAdsShowError error, string message)
        {
        }

        public void OnUnityAdsShowStart(string placementId)
        {
        }

        public void OnUnityAdsShowClick(string placementId)
        {
        }

        public void OnUnityAdsShowComplete(string placementId, UnityAdsShowCompletionState showCompletionState)
        {
            if (placementId == adUnitId && showCompletionState.Equals(UnityAdsShowCompletionState.COMPLETED))
            {
                Debug.Log("Ads Fully Watched .....");
                OnAdCompleted?.Invoke();
            }
        }

        #endregion


    }
}