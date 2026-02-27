using UnityEngine;
using UnityEngine.UI;

namespace ShipIt
{
    public class MusicManagerUI : MonoBehaviour
    {
        [SerializeField] MusicManager manager;
        [SerializeField] Slider volumeSlider;
        [SerializeField] Button muteButton;

        float lastUnmutedVolume = 1f;

        void Awake()
        {
            if (!manager)
            {
                manager = MusicManager.inst;
            }

            if (volumeSlider)
            {
                volumeSlider.minValue = 0f;
                volumeSlider.maxValue = 1f;
            }
        }

        void OnEnable()
        {
            if (volumeSlider)
            {
                volumeSlider.onValueChanged.AddListener(OnSliderChanged);
            }

            if (muteButton)
            {
                muteButton.onClick.AddListener(ToggleMute);
            }

            RefreshUIFromManager();
        }

        void OnDisable()
        {
            if (volumeSlider)
            {
                volumeSlider.onValueChanged.RemoveListener(OnSliderChanged);
            }

            if (muteButton)
            {
                muteButton.onClick.RemoveListener(ToggleMute);
            }
        }

        void OnSliderChanged(float sliderValue)
        {
            if (!manager)
            {
                return;
            }

            manager.SetVolume(sliderValue);

            if (sliderValue > 0f)
            {
                lastUnmutedVolume = sliderValue;
            }
        }

        public void ToggleMute()
        {
            if (!manager)
            {
                return;
            }

            float currentVolume = manager.Volume;

            if (currentVolume > 0f)
            {
                lastUnmutedVolume = currentVolume;
                SetVolume(0f);
                return;
            }

            SetVolume(Mathf.Clamp01(lastUnmutedVolume));
        }

        public void SetVolume(float volume)
        {
            if (!manager)
            {
                return;
            }

            manager.SetVolume(volume);

            if (volumeSlider && !Mathf.Approximately(volumeSlider.value, volume))
            {
                volumeSlider.SetValueWithoutNotify(volume);
            }
        }

        void RefreshUIFromManager()
        {
            if (!manager)
            {
                return;
            }

            float managerVolume = manager.Volume;

            if (volumeSlider)
            {
                volumeSlider.SetValueWithoutNotify(managerVolume);
            }

            if (managerVolume > 0f)
            {
                lastUnmutedVolume = managerVolume;
            }
        }
    }
}
