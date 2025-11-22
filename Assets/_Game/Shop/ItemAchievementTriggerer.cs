using System.Collections.Generic;
using UnityEngine;

namespace ShipIt.SocialNet
{
    public class ItemAchievementTriggerer : MonoBehaviour
    {
        enum Achievement
        {
            BoughtGreenShip, BoughtRedShip
        }
        
        [System.Serializable]
        class ItemAchivement
        {
            public ShopItem shopItem;
            public Achievement achievementName;
        }

        [SerializeField] List<ItemAchivement> achivements = new List<ItemAchivement>();
        [SerializeField] ShopManager shopManager;
        Dictionary<string, string> achievementsByItemId;
        GameSocials socials;
        
        void Start()
        {
            socials = GameSocials.inst;
            
            if(!socials || ! shopManager) return;

            shopManager.OnItemBought += TryTriggerAchievement;
            
            achievementsByItemId = new Dictionary<string, string>();
            for (int i = 0; i < achivements.Count; i++)
            {
                string id = "";
#if UNITY_ANDROID || PLATFORM_ANDROID
                switch (achivements[i].achievementName)
                {
                    case Achievement.BoughtRedShip:
                        id = GPGSIds.achievement_bought_red_ship;
                        break;
                    case Achievement.BoughtGreenShip:
                        id = GPGSIds.achievement_bought_green_ship;
                        break;
                }
#endif
                if(id == "") continue;
                achievementsByItemId.TryAdd(achivements[i].shopItem.ItemId, id);
            }
        }
        public void TryTriggerAchievement(ShopItem shopItem)
        {
            if(!achievementsByItemId.TryGetValue(shopItem.ItemId, out string id)) return;
            
            socials.UnlockAchievement(id);
        }
    }
}