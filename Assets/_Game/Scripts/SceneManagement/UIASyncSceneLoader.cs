using System;
using UnityEngine;

namespace Universal.SceneManaging
{
    public class UIASyncSceneLoader : MonoBehaviour
    {
        //[Header("Set Values")]
        //[Header("Runtime Values")]
        ASyncSceneLoader sceneLoader;

        //Unity Events
        void Start()
        {
            sceneLoader = ASyncSceneLoader.inst;
        }

        //Methods
        public void LoadScene(string sceneName)
        {
            sceneLoader.StartLoad(sceneName);
        }
        public void LoadScene(int sceneIndex)
        {
            sceneLoader.StartLoad(sceneIndex);
        }
    }
}
