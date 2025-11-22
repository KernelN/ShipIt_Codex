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
            if (!skinsManager) 
                skinsManager = SkinsManager.inst;
        }

        void Start()
        {
            ApplySelectedSkin();
        }

        void ApplySelectedSkin()
        {
            SkinOption option = skinsManager ? skinsManager.GetSelectedSkin() : null;
            if (!option || !option.Prefab)
            {
                return;
            }

            if (currentModelInstance)
            {
                Destroy(currentModelInstance);
            }

            Transform parent = modelParent ? modelParent : transform;
            currentModelInstance = Instantiate(option.Prefab, parent);
            currentModelInstance.transform.localPosition = Vector3.zero;
            currentModelInstance.transform.localRotation = Quaternion.identity;
            currentModelInstance.transform.localScale = Vector3.one;
        }

    }
}
