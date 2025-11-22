using UnityEngine;

namespace ShipIt.Gameplay
{
    public class ShipModel : MonoBehaviour
    {
        [SerializeField] Transform modelParent;
        [SerializeField] SkinsManager skinsManager;

        GameObject currentModelInstance;

        void Awake()
        {
            if (skinsManager == null)
            {
                skinsManager = SkinsManager.inst;
            }

            if (skinsManager == null)
            {
                skinsManager = FindObjectOfType<SkinsManager>();
            }
        }

        void Start()
        {
            ApplySelectedSkin();
        }

        void ApplySelectedSkin()
        {
            SkinOption option = skinsManager != null ? skinsManager.GetSelectedSkin() : null;
            if (option == null || option.Prefab == null)
            {
                return;
            }

            if (currentModelInstance != null)
            {
                Destroy(currentModelInstance);
            }

            Transform parent = modelParent != null ? modelParent : transform;
            currentModelInstance = Instantiate(option.Prefab, parent);
            currentModelInstance.transform.localPosition = Vector3.zero;
            currentModelInstance.transform.localRotation = Quaternion.identity;
            currentModelInstance.transform.localScale = Vector3.one;
        }

    }
}
