using System.Collections.Generic;
using UnityEngine;

namespace ShipIt
{
    public class ShopManagerUI : MonoBehaviour
    {
        [System.Serializable]
        public class ShopEntry
        {
            public ShopItem item;
            public ShopItemUI ui;
        }

        [SerializeField] ShopManager shopManager;
        [SerializeField] List<ShopEntry> shopEntries = new List<ShopEntry>();
        [SerializeField] Color permanentOwnedColor = Color.grey;
        [SerializeField] Color selectedItemColor = Color.cyan;

        public System.Action<int> OnCreditsChanged;
        public System.Action<ShopItem> OnItemBought;
        public System.Action<ShopItem> OnItemSelected;

        bool subscribed;
        
        void Start()
        {
            SubscribeToShopManager();
            InitializeEntries();
        }
        void OnDestroy()
        {
            UnsubscribeFromShopManager();
        }
        public bool CanBuy(ShopItem item) 
            => shopManager && shopManager.CanBuy(item);

        public bool TryBuy(ShopItem item)
        {
            if (!shopManager) return false;

            bool bought = shopManager.TryBuy(item);

            if (bought) 
                UpdateEntry(item);

            return bought;
        }
        public int GetOwnedQuantity(ShopItem item)
        {
            return shopManager ? shopManager.GetOwnedQuantity(item) : 0;
        }
        public bool IsSelected(ShopItem item)
        {
            return shopManager && shopManager.IsSelected(item);
        }
        public bool TrySelect(ShopItem item)
        {
            if (!shopManager) return false;

            bool selected = shopManager.TrySelect(item);

            if (selected)
            {
                RefreshAll();
                OnSelectionChanged(item);
            }

            return selected;
        }

        public int GetCredits()
        {
            return shopManager ? shopManager.GetCredits() : 0;
        }

        void InitializeEntries()
        {
            foreach (ShopEntry entry in shopEntries)
                if (entry.ui && entry.item) 
                    entry.ui.Initialize(this, entry.item, selectedItemColor, permanentOwnedColor);
        }

        void UpdateEntry(ShopItem item)
        {
            foreach (ShopEntry entry in shopEntries)
                if (entry.item == item && entry.ui)
                {
                    entry.ui.RefreshUI();
                    break;
                }
        }

        void HandleCreditsChanged(int credits)
        {
            OnCreditsChanged?.Invoke(credits);
            RefreshAll();
        }

        void HandleItemBought(ShopItem item)
        {
            OnItemBought?.Invoke(item);
            UpdateEntry(item);
        }

        void HandleSelectionChanged(ShopItem item)
        {
            OnSelectionChanged(item);
            RefreshAll();
        }

        void OnSelectionChanged(ShopItem item)
        {
            OnItemSelected?.Invoke(item);
        }

        void RefreshAll()
        {
            foreach (ShopEntry entry in shopEntries)
            {
                if (entry.ui != null)
                {
                    entry.ui.RefreshUI();
                }
            }
        }

        void SubscribeToShopManager()
        {
            if (shopManager == null || subscribed)
            {
                return;
            }

            shopManager.OnCreditsChanged += HandleCreditsChanged;
            shopManager.OnItemBought += HandleItemBought;
            shopManager.OnItemSelected += HandleSelectionChanged;
            subscribed = true;
        }

        void UnsubscribeFromShopManager()
        {
            if (shopManager == null || !subscribed)
            {
                return;
            }

            shopManager.OnCreditsChanged -= HandleCreditsChanged;
            shopManager.OnItemBought -= HandleItemBought;
            shopManager.OnItemSelected -= HandleSelectionChanged;
            subscribed = false;
        }
    }
}
