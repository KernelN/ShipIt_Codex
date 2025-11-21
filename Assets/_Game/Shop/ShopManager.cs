using UnityEngine;

namespace ShipIt
{
    public class ShopManager : MonoBehaviour
    {
        public System.Action<int> OnCreditsChanged;
        public System.Action<ShopItem> OnItemBought;

        GameManager manager;

        void Awake()
        {
            manager = GameManager.inst;
            if(manager && manager.Data.items == null)
                manager.Data.items = new System.Collections.Generic.List<ItemData>();
        }
        public bool CanBuy(ShopItem item)
        {
            if (item == null || manager == null || manager.Data == null)
            {
                return false;
            }

            bool ownsPermanent = !item.IsSpendable && GetOwnedQuantity(item) > 0;
            bool hasCredits = manager.Data.credits >= item.Cost;

            return hasCredits && !ownsPermanent;
        }
        public bool TryBuy(ShopItem item)
        {
            if (item == null || manager == null || manager.Data == null)
            {
                return false;
            }

            if (!CanBuy(item))
            {
                return false;
            }

            ItemData record = manager.Data.items.Find(p => p.id == item.ItemId);

            if (record == null)
            {
                record = new ItemData { id = item.ItemId };
                manager.Data.items.Add(record);
            }

            manager.Data.credits -= item.Cost;

            if (item.IsSpendable)
            {
                record.quantity += 1;
            }
            else
            {
                record.quantity = 1;
            }

            manager.SaveGameData();

            OnItemBought?.Invoke(item);
            OnCreditsChanged?.Invoke(manager.Data.credits);

            return true;
        }
        public int GetOwnedQuantity(ShopItem item)
        {
            if (item == null || manager == null || manager.Data == null)
            {
                return 0;
            }

            ItemData record = manager.Data.items.Find(p => p.id == item.ItemId);
            return record?.quantity ?? 0;
        }
        public int GetCredits()
        {
            if (manager == null || manager.Data == null)
            {
                return 0;
            }

            return manager.Data.credits;
        }
    }
}
