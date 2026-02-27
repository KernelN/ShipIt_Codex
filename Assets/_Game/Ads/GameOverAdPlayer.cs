using UnityEngine;
using Universal.Ads;

namespace ShipIt.Ads
{
    public class GameOverAdPlayer : MonoBehaviour
    {
        [SerializeField] AdController ad;
        void Start()
        {
            Gameplay.GameplayManager.inst.OnOrderCompleted += ad.ShowAd;
            Gameplay.GameplayManager.inst.OnOrderFailed += ad.ShowAd;
        }
    }
}