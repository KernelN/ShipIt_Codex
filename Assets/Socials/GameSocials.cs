using UnityEngine;
using GooglePlayGames;

namespace ShipIt.SocialNet
{
    public class GameSocials : Universal.Singleton<GameSocials>
    {
        internal override bool DoNotDestroyOnLoad => true;

        string leaderboardID = "";
#if UNITY_ANDROID || PLATFORM_ANDROID
        public static PlayGamesPlatform platform = null;
#endif
        
        //Unity Events
        void Start()
        {
#if UNITY_EDITOR || PLATFORM_STANDALONE
            return;
#endif
            
            string gameStartedId;
            
#if UNITY_ANDROID || PLATFORM_ANDROID
            if (platform == null)
            {
                PlayGamesPlatform.DebugLogEnabled = true;
                platform = PlayGamesPlatform.Activate();
                Debug.Log("GPGS - Play Games activated successfully");
            }
            else 
                Debug.Log("GPGS - Play Games activation failed");

            gameStartedId = GPGSIds.achievement_game_started;
#endif
            
            LogIn();

            
            
            UnlockAchievement(gameStartedId);
        }

        //Methods
        public void LogIn(bool logInFromPlatform = false)
        {
#if UNITY_EDITOR || PLATFORM_STANDALONE
            return;
#endif
            if (logInFromPlatform)
            {
#if UNITY_ANDROID || PLATFORM_ANDROID
                platform.Authenticate(OnGoogleLogIn);
#endif
            }
            else
                Social.localUser.Authenticate(OnUnityLogIn);
        }
        public void AddScoreToLeaderboard(int score)
        {
#if UNITY_EDITOR || PLATFORM_STANDALONE
            return;
#endif
            if (score % 5 != 0)
            {
                Debug.Log("GameSocials ERROR - Score must be a multiple of 5");
                return;
            }
            if (Social.localUser.authenticated)
            {
                Social.ReportScore(score, leaderboardID, success => { });
            }
        }
        public void ShowLeaderboard()
        {
#if UNITY_EDITOR || PLATFORM_STANDALONE
            return;
#endif
            if (Social.localUser.authenticated)
            {
#if UNITY_ANDROID || PLATFORM_ANDROID
                platform.ShowLeaderboardUI();
#endif
            }
        }
        public void ShowAchievements()
        {
#if UNITY_EDITOR || PLATFORM_STANDALONE
            return;
#endif
            if (Social.localUser.authenticated)
            {
#if UNITY_ANDROID || PLATFORM_ANDROID
                platform.ShowAchievementsUI();
#endif
            }
        }
        public void UnlockAchievement(string id, bool unlockFromPlatform = false)
        {
#if UNITY_EDITOR || PLATFORM_STANDALONE
            return;
#endif
            if (unlockFromPlatform)
            {
#if UNITY_ANDROID || PLATFORM_ANDROID
                platform.UnlockAchievement(id);
#endif
                return;
            }
            
            if (Social.localUser.authenticated)
                Social.ReportProgress(id, 100f, success => { });
            else
                Debug.Log("GameSocials - Failed to unlock achievement");
            
        }
        void OnUnityLogIn(bool success)
        {
#if UNITY_EDITOR || PLATFORM_STANDALONE
            return;
#endif
            if (success)
            {
                Debug.Log("GameSocials - Logged in successfully");
                Debug.Log("GameSocials - ID: " + Social.localUser.id);
                Debug.Log("GameSocials - Name: " + Social.localUser.userName);
            }
            else
            {
                Debug.Log("GameSocials - Failed to login");
            }
        }
#if UNITY_ANDROID || PLATFORM_ANDROID
        void OnGoogleLogIn(GooglePlayGames.BasicApi.SignInStatus signInStatus)
        {
            if (signInStatus == GooglePlayGames.BasicApi.SignInStatus.Success)
            {
                Debug.Log("GPGS - Logged in successfully");
                Debug.Log("GPGS - ID: " + platform.localUser.id);
                Debug.Log("GPGS - Name: " + platform.localUser.userName);
            }
            else
            {
                Debug.Log("GPGS - Failed to login: " + signInStatus);
            }
        }
#endif
    }
}