using System.Collections;
using UnityEngine;
using Universal.FileManaging;

namespace ShipIt
{
    public class MusicManager : Universal.Singleton<MusicManager>
    {
        internal override bool DoNotDestroyOnLoad => true;

        const string ConfigDataPath = "/Data/ConfigData.dat";

        [SerializeField, Min(0f)] float fadeDuration = 1f;
        [SerializeField] AudioSource firstSource;
        [SerializeField] AudioSource secondSource;

        ConfigData configData;
        Coroutine fadeRoutine;
        AudioSource activeSource;
        AudioSource inactiveSource;

        public float Volume => configData != null ? configData.musicVolume : 1f;

        internal override void Awake()
        {
            base.Awake();

            if (this != inst)
            {
                return;
            }

            LoadConfigData();
            SetupSources();
        }

        internal override void OnDestroy()
        {
            if (this == inst)
            {
                SaveConfigData();
            }

            base.OnDestroy();
        }

        public void RequestTrack(AudioClip newClip)
        {
            if (!newClip || !firstSource || !secondSource)
            {
                return;
            }

            if (activeSource != null && activeSource.clip == newClip)
            {
                return;
            }

            if (fadeRoutine != null)
            {
                StopCoroutine(fadeRoutine);
            }

            fadeRoutine = StartCoroutine(FadeToTrack(newClip));
        }

        public void SetVolume(float volume)
        {
            if (configData == null)
            {
                configData = new ConfigData();
            }

            float previousVolume = configData.musicVolume;
            float targetVolume = Mathf.Clamp01(volume);

            configData.musicVolume = targetVolume;
            ApplyVolume(previousVolume, targetVolume);
            SaveConfigData();
        }

        void SetupSources()
        {
            if (!firstSource || !secondSource)
            {
                return;
            }

            firstSource.loop = true;
            secondSource.loop = true;

            activeSource = firstSource;
            inactiveSource = secondSource;

            firstSource.volume = 0f;
            secondSource.volume = 0f;
        }

        IEnumerator FadeToTrack(AudioClip newClip)
        {
            if (inactiveSource == null || activeSource == null)
            {
                yield break;
            }

            inactiveSource.clip = newClip;
            inactiveSource.volume = 0f;
            inactiveSource.Play();

            if (fadeDuration <= 0f)
            {
                activeSource.Stop();
                activeSource.volume = 0f;
                inactiveSource.volume = Volume;
                SwapSources();
                yield break;
            }

            float elapsed = 0f;

            while (elapsed < fadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / fadeDuration);

                activeSource.volume = Mathf.Lerp(Volume, 0f, t);
                inactiveSource.volume = Mathf.Lerp(0f, Volume, t);

                yield return null;
            }

            activeSource.Stop();
            activeSource.volume = 0f;
            inactiveSource.volume = Volume;

            SwapSources();
            fadeRoutine = null;
        }

        void SwapSources()
        {
            AudioSource oldActive = activeSource;
            activeSource = inactiveSource;
            inactiveSource = oldActive;
        }

        void ApplyVolume(float previousVolume, float targetVolume)
        {
            if (activeSource && activeSource.isPlaying)
            {
                activeSource.volume = RemapVolume(activeSource.volume, previousVolume, targetVolume);
            }

            if (inactiveSource && inactiveSource.isPlaying)
            {
                inactiveSource.volume = RemapVolume(inactiveSource.volume, previousVolume, targetVolume);
            }
        }

        float RemapVolume(float currentChannelVolume, float previousVolume, float targetVolume)
        {
            if (Mathf.Approximately(previousVolume, 0f))
            {
                return targetVolume;
            }

            float blendPercent = Mathf.Clamp01(currentChannelVolume / previousVolume);
            return blendPercent * targetVolume;
        }

        void LoadConfigData()
        {
            string path = Application.persistentDataPath + ConfigDataPath;
            configData = FileManager<ConfigData>.LoadDataFromFile(path) ?? new ConfigData();
            configData.musicVolume = Mathf.Clamp01(configData.musicVolume);
        }

        void SaveConfigData()
        {
            string path = Application.persistentDataPath + ConfigDataPath;
            FileManager<ConfigData>.SaveDataToFile(configData, path);
        }
    }
}
