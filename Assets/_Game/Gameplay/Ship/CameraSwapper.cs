using UnityEngine;
using UnityEngine.UI;

namespace ShipIt.Gameplay
{
    public class CameraSwapper : MonoBehaviour
    {
        [Header("Camera setup")]
        [SerializeField] Camera firstCamera;
        [SerializeField] Camera secondCamera;

        [Header("Optional UI binding")]
        [SerializeField] Button swapButton;

        [Header("Animation setup")]
        [SerializeField] Animator targetAnimator;
        [SerializeField] string triggerName = "Play";

        bool showFirstCamera = true;
        public Camera cam => showFirstCamera ? firstCamera : secondCamera;

        void Awake()
        {
            ApplyCameraState();
        }

        void OnEnable()
        {
            if (swapButton)
                swapButton.onClick.AddListener(SwapCameraAndPlayAnimation);
        }

        void OnDisable()
        {
            if (swapButton)
                swapButton.onClick.RemoveListener(SwapCameraAndPlayAnimation);
        }

        public void SwapCameraAndPlayAnimation()
        {
            showFirstCamera = !showFirstCamera;
            ApplyCameraState();

            if (!targetAnimator || string.IsNullOrEmpty(triggerName))
                return;

            targetAnimator.ResetTrigger(triggerName);
            targetAnimator.SetTrigger(triggerName);
        }

        void ApplyCameraState()
        {
            if (firstCamera)
                firstCamera.gameObject.SetActive(showFirstCamera);

            if (secondCamera)
                secondCamera.gameObject.SetActive(!showFirstCamera);
        }
    }
}
