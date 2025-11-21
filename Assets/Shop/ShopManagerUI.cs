using System;
using System.Collections.Generic;
using UnityEngine;

namespace ShipIt
{
    public class ShopManagerUI : MonoBehaviour
    {
        [Serializable]
        public class ShopEntry
        {
            public ShopItem item;
            public ShopItemUI ui;
        }

        [SerializeField] ShopManager shopManager;
        [SerializeField] List<ShopEntry> shopEntries = new List<ShopEntry>();
        [SerializeField] Color defaultItemColor = Color.white;
        [SerializeField] Color permanentOwnedColor = Color.green;

        public event Action<int> OnCreditsChanged;
        public event Action<ShopItem> OnItemBought;

        bool subscribed;

        void Awake()
        {
            if (shopManager == null)
            {
                shopManager = GetComponent<ShopManager>();
            }
        }
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
        {
            return shopManager != null && shopManager.CanBuy(item);
        }

        public bool TryBuy(ShopItem item)
        {
            if (shopManager == null)
            {
                return false;
            }

            bool bought = shopManager.TryBuy(item);

            if (bought)
            {
                UpdateEntry(item);
            }

            return bought;
        }

        public int GetOwnedQuantity(ShopItem item)
        {
            return shopManager != null ? shopManager.GetOwnedQuantity(item) : 0;
        }

        public int GetCredits()
        {
            return shopManager != null ? shopManager.GetCredits() : 0;
        }

        void InitializeEntries()
        {
            foreach (ShopEntry entry in shopEntries)
            {
                if (entry.ui != null && entry.item != null)
                {
                    entry.ui.Initialize(this, entry.item, defaultItemColor, permanentOwnedColor);
                }
            }
        }

        void UpdateEntry(ShopItem item)
        {
            foreach (ShopEntry entry in shopEntries)
            {
                if (entry.item == item && entry.ui != null)
                {
                    entry.ui.RefreshUI();
                    break;
                }
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
            subscribed = false;
        }
    }
}
