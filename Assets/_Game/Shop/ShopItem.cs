using UnityEngine;

namespace ShipIt
{
    [CreateAssetMenu(menuName = "ShipIt/Shop Item", fileName = "ShopItem")]
    public class ShopItem : ScriptableObject
    {
        [SerializeField] string displayName;
        [SerializeField] string itemId;
        [SerializeField] int cost;
        [SerializeField] bool isSpendable;

        public string DisplayName => displayName;
        public string ItemId => itemId;
        public int Cost => cost;
        public bool IsSpendable => isSpendable;
    }
}
