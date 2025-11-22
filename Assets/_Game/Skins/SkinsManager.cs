using System;
using System.Collections.Generic;
using ShipIt;
using UnityEngine;

namespace ShipIt.Gameplay
{
    public class SkinsManager : Universal.Singleton<SkinsManager>
    {
        [SerializeField] SkinOption defaultSkin;
        [SerializeField] List<SkinOption> availableSkins = new List<SkinOption>();
        public event Action OnSkinChanged;

        string selectedSkinId;

        internal override void Awake()
        {
            base.Awake();
        }

        void Start()
        {
            if (this != inst)
            {
                return;
            }

            selectedSkinId = LoadPersistedSelection();
        }

        public bool TrySelectSkin(SkinOption option)
        {
            if (option == null)
            {
                return false;
            }

            GameManager manager = GameManager.inst;
            GameData data = manager != null ? manager.Data : null;

            if (!PlayerOwnsItem(option, data))
            {
                return false;
            }

            if (selectedSkinId == option.ItemId)
            {
                return false;
            }

            selectedSkinId = option.ItemId;

            OnSkinChanged?.Invoke();

            if (data != null)
            {
                data.selectedSkinId = selectedSkinId;
                manager.SaveGameData();
            }

            return true;
        }

        public bool IsSkinSelected(SkinOption option)
        {
            return option != null && option.ItemId == GetSelectedSkinId();
        }

        public SkinOption GetSelectedSkin()
        {
            GameManager manager = GameManager.inst;
            GameData data = manager != null ? manager.Data : null;

            string selectedId = GetSelectedSkinId();

            if (!string.IsNullOrEmpty(selectedId))
            {
                SkinOption ownedSelection = FindOwnedOption(selectedId, data);
                if (ownedSelection != null)
                {
                    return ownedSelection;
                }
            }

            return defaultSkin != null && defaultSkin.Prefab != null ? defaultSkin : null;
        }

        SkinOption FindOwnedOption(string itemId, GameData data)
        {
            foreach (SkinOption option in availableSkins)
            {
                if (option == null || option.ItemId != itemId)
                {
                    continue;
                }

                if (data != null && PlayerOwnsItem(option, data))
                {
                    return option;
                }

                break;
            }

            return null;
        }

        static bool PlayerOwnsItem(ShopItem item, GameData data)
        {
            if (item == null || data == null || data.items == null)
            {
                return false;
            }

            ItemData record = data.items.Find(entry => entry.id == item.ItemId);
            return record != null && record.quantity > 0;
        }

        string GetSelectedSkinId()
        {
            if (string.IsNullOrEmpty(selectedSkinId))
            {
                selectedSkinId = LoadPersistedSelection();
            }

            return selectedSkinId;
        }

        string LoadPersistedSelection()
        {
            GameManager manager = GameManager.inst;
            GameData data = manager != null ? manager.Data : null;

            return data != null ? data.selectedSkinId : null;
        }
    }
}
