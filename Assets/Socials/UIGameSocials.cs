using UnityEngine;

namespace ShipIt.SocialNet
{
    public class UIGameSocials : MonoBehaviour
    {
        //[Header("Set Values")]
        //[Header("Runtime Values")]
        GameSocials socials;
        string aID = GPGSIds.achievement_game_started; //test achievement ID

        //Unity Events
        void Start()
        {
            socials = GameSocials.inst;
            if (!socials)
            {
                Debug.Log("UIGameSocials - GameSocials not found");
                Destroy(this);
            }
        }

        //Methods
        public void LogIn() => socials.LogIn();
        public void LogInFromGoogle() => socials.LogIn(true);
        public void UnlockAchievement() => socials.UnlockAchievement(aID);
        public void UnlockAchievementFromGoogle() => socials.UnlockAchievement(aID, true);
    }
}
