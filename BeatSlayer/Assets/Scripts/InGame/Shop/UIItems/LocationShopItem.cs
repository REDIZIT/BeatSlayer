using Assets.SimpleLocalization;
using GameNet;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace InGame.Shop.UIItems
{
    public class LocationShopItem : MonoBehaviour
    {
        public ShopHelper shopHelper;

        public LocationSO item;
        private Prefs prefs;


        [Header("UI")]
        public Image image;
        public Text nameText, costText;
        public GameObject buySection, selectSection;

        [Header("Buy section")]
        public RectTransform sizedContainer, coinTransform;
        private float coinSpacing = 22;

        [Header("Select section")]
        public Button selectBtn;
        public Button buyBtn;
        private Func<string, string> Local => LocalizationManager.Localize;



        public void Refresh(ShopItemSO shopItem, Prefs prefs)
        {
            item = (LocationSO)shopItem;
            this.prefs = prefs;

            //nameText.text = Local(item.name);
            image.sprite = item.image;
            costText.text = item.cost + "";

            bool isBought = shopHelper.IsPurchased(shopItem);

            RefreshSections(isBought);

            if (Payload.Account != null)
                buyBtn.interactable = Payload.Account.Coins >= shopItem.cost;
        }
        public void Refresh()
        {
            Refresh(item, prefs);
        }

        public async void OnBuyBtnClick()
        {
            await shopHelper.BuyLocation(item);
        }

        public void OnSelectBtnClicked()
        {
            shopHelper.SelectLocation(item);
        }





        private void RefreshSections(bool isBought)
        {
            buySection.SetActive(!isBought);
            selectSection.SetActive(isBought);
            selectBtn.interactable = prefs.selectedMapId != item.id;

            RefreshCoinsContainer();
        }


        private void RefreshCoinsContainer()
        {
            float coinWidth = coinTransform.rect.width + coinSpacing;
            float textWidth = costText.preferredWidth;

            float containerWidth = coinWidth + textWidth;
            sizedContainer.sizeDelta = new Vector2(containerWidth, sizedContainer.sizeDelta.y);
            sizedContainer.anchoredPosition = new Vector3(0, 0);

            coinTransform.anchoredPosition = new Vector2(textWidth / 2f + coinSpacing, 0);
        }
    }
}
