using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ShipIt
{
    public class ShopItemUI : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI nameLabel;
        [SerializeField] TextMeshProUGUI costLabel;
        [SerializeField] TextMeshProUGUI statusLabel;
        [SerializeField] Button buyButton;
        [SerializeField] Image background;

        ShopItem item;
        ShopManagerUI managerUI;
        bool subscribed;
        Color defaultColor = Color.white;
        Color permanentOwnedColor = Color.green;

        void OnEnable()
        {
            Subscribe();
        }

        void OnDisable()
        {
            Unsubscribe();
        }

        public void Initialize(ShopManagerUI shopManagerUI, ShopItem shopItem, Color defaultItemColor, Color permanentColor)
        {
            managerUI = shopManagerUI;
            item = shopItem;
            defaultColor = defaultItemColor;
            permanentOwnedColor = permanentColor;
            
            if(!item) return;
            
            if (nameLabel) 
                nameLabel.text = item.DisplayName;

            if (costLabel) 
                costLabel.text = item.Cost.ToString("$0");

            RefreshUI();
            Subscribe();
        }

        void BuyItem()
        {
            if (managerUI == null || item == null)
            {
                return;
            }

            managerUI.TryBuy(item);
        }

        void HandleCreditsChanged(int _)
        {
            RefreshUI();
        }

        void HandleItemBought(ShopItem purchasedItem)
        {
            if (item != null && purchasedItem == item)
            {
                RefreshUI();
            }
        }

        public void RefreshUI()
        {
            if (managerUI == null || item == null)
            {
                return;
            }

            int owned = managerUI.GetOwnedQuantity(item);

            if (statusLabel != null)
            {
                bool showStatus = item.IsSpendable;
                statusLabel.gameObject.SetActive(showStatus);

                if (showStatus)
                {
                    statusLabel.text = $"Owned: {owned}";
                }
            }

            if (background != null && !item.IsSpendable)
            {
                background.color = owned > 0 ? permanentOwnedColor : defaultColor;
            }

            if (buyButton != null)
            {
                buyButton.interactable = managerUI.CanBuy(item);
            }
        }

        void Subscribe()
        {
            if (managerUI == null || subscribed)
            {
                return;
            }

            managerUI.OnCreditsChanged += HandleCreditsChanged;
            managerUI.OnItemBought += HandleItemBought;
            subscribed = true;

            RefreshUI();
        }

        void Unsubscribe()
        {
            if (managerUI == null || !subscribed)
            {
                return;
            }

            managerUI.OnCreditsChanged -= HandleCreditsChanged;
            managerUI.OnItemBought -= HandleItemBought;
            subscribed = false;
        }
    }
}
