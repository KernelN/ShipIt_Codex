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
        Color permanentOwnedColor = Color.green;
        Color selectedColor = Color.cyan;

        void Start() => Subscribe();
        void OnDestroy() => Unsubscribe();

        public void Initialize(ShopManagerUI shopManagerUI, ShopItem shopItem, Color selectedColor, Color permanentColor)
        {
            managerUI = shopManagerUI;
            item = shopItem;
            this.selectedColor = selectedColor;
            permanentOwnedColor = permanentColor;
            
            if(!item) return;
            
            if (nameLabel) 
                nameLabel.text = item.DisplayName;

            if (costLabel) 
                costLabel.text = item.Cost.ToString("$0");

            RefreshUI();
            Subscribe();
        }
        public void HandleAction()
        {
            if (!managerUI || !item) return;

            if (item.IsSpendable)
            {
                managerUI.TryBuy(item);
                return;
            }

            int owned = managerUI.GetOwnedQuantity(item);

            if (owned <= 0)
            {
                managerUI.TryBuy(item);
                return;
            }

            managerUI.TrySelect(item);
        }
        void HandleCreditsChanged(int _) => RefreshUI();
        void HandleItemBought(ShopItem purchasedItem)
        {
            if (item && purchasedItem == item) 
                RefreshUI();
        }
        void HandleItemSelected(ShopItem selectedItem)
        {
            if (!selectedItem || !item) return;

            if (selectedItem == item || managerUI.IsSelected(item)) 
                RefreshUI();
        }
        public void RefreshUI()
        {
            if (!managerUI || !item) return;

            int owned = managerUI.GetOwnedQuantity(item);
            bool isSelected = managerUI.IsSelected(item);

            if (costLabel && !item.IsSpendable && owned > 0)
                costLabel.fontStyle = FontStyles.Strikethrough;

            if (statusLabel)
            {
                if (item.IsSpendable)
                {
                    statusLabel.gameObject.SetActive(true);
                    statusLabel.text = $"Owned: {owned}";
                }
                else
                {
                    statusLabel.gameObject.SetActive(isSelected);

                    if (isSelected) 
                        statusLabel.text = "Selected";
                }
            }

            if (background)
            {
                if (isSelected)
                    background.color = selectedColor;
                else if (!item.IsSpendable && owned > 0)
                    background.color = permanentOwnedColor;
            }

            if (buyButton)
            {
                bool canBuy = managerUI.CanBuy(item);
                buyButton.interactable = owned > 0 ? !isSelected : canBuy;
            }
        }
        void Subscribe()
        {
            if (!managerUI || subscribed) return;

            managerUI.OnCreditsChanged += HandleCreditsChanged;
            managerUI.OnItemBought += HandleItemBought;
            managerUI.OnItemSelected += HandleItemSelected;
            subscribed = true;

            RefreshUI();

            if (buyButton) 
                buyButton.onClick.AddListener(HandleAction);
        }
        void Unsubscribe()
        {
            if (!managerUI || !subscribed) return;

            managerUI.OnCreditsChanged -= HandleCreditsChanged;
            managerUI.OnItemBought -= HandleItemBought;
            managerUI.OnItemSelected -= HandleItemSelected;
            subscribed = false;

            if (buyButton) 
                buyButton.onClick.RemoveListener(HandleAction);
        }
    }
}