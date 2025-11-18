using System;
using System.Collections.Generic;
using UnityEngine;

public class SplashScreenRunner : MonoBehaviour
{
    enum State { Fade, Stay }
    [System.Serializable]
    class SplashScreen
    {
        public float duration;
        public CanvasGroup canvasGroup;
    }
    [SerializeField] List<SplashScreen> splashScreens;
    [SerializeField] AnimationCurve fadeIn = AnimationCurve.EaseInOut(0,0,1,1);
    [SerializeField] AnimationCurve fadeInBetween = AnimationCurve.EaseInOut(0,0,1,1);
    [SerializeField] AnimationCurve fadeOut = AnimationCurve.EaseInOut(0,1,1,0);
    [SerializeField] float fadeTime;
    [SerializeField] string nextScene;
    int index;
    float timer;
    State state;

    void Start()
    {
        Universal.SceneManaging.ASyncSceneLoader.inst.StartLoadAndHold(nextScene);
        timer = 0;
        state = State.Fade;
        index = 0;
    }
    void Update()
    {
        timer += Time.deltaTime;
        switch (state)
        {
            case State.Fade:
                float t = timer / fadeTime;
                if (index == 0)
                    splashScreens[0].canvasGroup.alpha = fadeIn.Evaluate(t);
                else if(index == splashScreens.Count)
                    splashScreens[^1].canvasGroup.alpha = fadeOut.Evaluate(t);
                else
                {
                    t = fadeInBetween.Evaluate(t);
                    splashScreens[index - 1].canvasGroup.alpha = 1 - t;
                    splashScreens[index].canvasGroup.alpha = t;
                }

                if (timer >= fadeTime)
                {
                    state = State.Stay;
                    timer = 0;
                    if (index == splashScreens.Count) 
                        Universal.SceneManaging.ASyncSceneLoader.inst.EnableSceneLoad();
                }
                break;
            case State.Stay:
                if (timer >= splashScreens[index].duration)
                {
                    state = State.Fade;
                    timer = 0;
                    index++;
                }
                break;
        }
            
    }
}
