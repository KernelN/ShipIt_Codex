using UnityEngine;

namespace ShipIt.Gameplay
{
    [CreateAssetMenu(menuName = "ShipIt/Shop/Skin Option", fileName = "SkinOption")]
    public class SkinOption : ShopItem
    {
        [SerializeField] GameObject prefab;

        public GameObject Prefab => prefab;
    }
}
