using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Universal.SceneManaging
{
    public class ASyncSceneLoader : MonoBehaviour
    {
        public static ASyncSceneLoader inst { get; protected set; }
        public string sceneLoading { get; private set; }
        [SerializeField] float minLoadSeconds;
        bool loadIsDone;
        bool waitForCommand;

        //Unity Events
        void Awake()
        {
            if (inst != null)
            {
                Destroy(this);
                return;
            }
            
            inst = this;
            DontDestroyOnLoad(this);
        }
        void OnDestroy()
        {
            if (inst == this)
                inst = null;
        }

        //Methods
        public void StartLoad(string sceneToLoad, bool useLoadingScene = true)
        {
            sceneLoading = sceneToLoad;
            waitForCommand = false;
            Load(useLoadingScene);
        }
        public void StartLoad(int sceneToLoad, bool useLoadingScene = true)
        {
            sceneLoading = SceneManager.GetSceneByBuildIndex(sceneToLoad).name;
            waitForCommand = false;
            Load(useLoadingScene);
        }
        public void StartLoadAndHold(string sceneToLoad, bool useLoadingScene = false)
        {
            sceneLoading = sceneToLoad;
            waitForCommand = true;
            Load(useLoadingScene);
        }
        public void StartLoadAndHold(int sceneToLoad, bool useLoadingScene = false)
        {
            sceneLoading = SceneManager.GetSceneByBuildIndex(sceneToLoad).name;
            waitForCommand = true;
            Load(useLoadingScene);
        }
        void Load(bool useLoadingScene)
        {
            if(useLoadingScene)
                SceneManager.LoadScene("LoadScene");
            StartCoroutine(LoadAsyncScene());
        }
        public void EnableSceneLoad() => waitForCommand = false;
        IEnumerator LoadAsyncScene()
        {
            yield return null; //wait 1 tick so LoadScene finishes loading
            
            //The Application loads the Scene in the background as the current Scene runs.
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneLoading);
            asyncLoad.allowSceneActivation = false;

            //Set timer
            float timer = 0;

            // Wait until the asynchronous scene fully loads
            do
            {
                timer += Time.unscaledDeltaTime;
                //Debug.Log("T: " + timer + " | P: " + asyncLoad.progress);
                yield return null;
            } while (timer < minLoadSeconds);

            while(waitForCommand) yield return null;
            
            //Debug.Log("Finished loading " + sceneLoading);
            asyncLoad.allowSceneActivation = true;
        }
    }
}