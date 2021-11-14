using Assets.SimpleLocalization;
using GameNet;
using InGame.Shop.UIItems;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace InGame.Shop
{
    public class SaberShopItem : BasicShopItem
    {
        public ShopHelper shopHelper;

        public SaberSO item;
        private Prefs prefs;
        

        [Header("UI")]
        public Image image;
        public Text nameText, descriptionText, costText;
        public GameObject buySection, selectSection;
        public Image rightOutline, leftOutline, bothOutline;
        public Image rightBg, leftBg, bothBg;
        public Image rightImage, leftImage, bothLeftImage, bothRightImage;
        [SerializeField] private GameObject winterRewardText;

        [Header("Buy section")]
        public float coinSpacing;
        public Button buyBtn;
        public RectTransform sizedContainer, coinTransform;

        private Func<string, string> Local => LocalizationManager.Localize;


        public void Refresh(ShopItemSO shopItem, Prefs prefs)
        {
            item = (SaberSO)shopItem;
            this.prefs = prefs;

            nameText.text = Local(item.name);
            descriptionText.text = Local(item.description);
            image.sprite = item.image;
            costText.text = item.cost + "";


            bool isBought = shopHelper.IsPurchased(shopItem);
            RefreshSections(isBought, shopItem.cost);

            if(Payload.Account != null)
                buyBtn.interactable = Payload.Account.Coins >= shopItem.cost;

            RefreshSelection(prefs);
        }
        public void Refresh()
        {
            Refresh(item, prefs);
        }

        public async void OnBuyBtnClick()
        {
            buyBtn.interactable = false;
            await shopHelper.BuySaber(item.id);
            buyBtn.interactable = true;
            Refresh();
        }
        /// <summary>
        /// On unity button click (left, both, right)
        /// </summary>
        /// <param name="hand">Left: -1, Right: 1, Both: 0</param>
        public void OnSelectBtnClick(int handIndex)
        {
            SaberHand hand = (SaberHand)handIndex;

            shopHelper.SelectSaber(item.id, hand);
        }





        private void RefreshSections(bool isBought, int cost)
        {
            buySection.SetActive(!isBought && cost >= 0);
            selectSection.SetActive(isBought);
            winterRewardText.SetActive(cost < 0);

            RefreshCoinsContainer();
        }

        private void RefreshSelection(Prefs prefs)
        {
            bool isLeft = prefs.selectedLeftSaberId == item.id;
            bool isRight = prefs.selectedRightSaberId == item.id;

            if (isLeft && isRight) EnableSelectionBoth();
            else if (isLeft) EnableSelectionLeft();
            else if (isRight) EnableSelectionRight();
            else DisableSelection();
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




        private void DisableSelection()
        {
            Colorize(rightOutline, SSytem.rightColor, false);
            Colorize(leftOutline, SSytem.leftColor, false);
            Colorize(bothOutline, Color.white, false);

            Colorize(rightImage, SSytem.rightColor, false);
            Colorize(leftImage, SSytem.leftColor, false);
            Colorize(bothLeftImage, SSytem.leftColor, false);
            Colorize(bothRightImage, SSytem.rightColor, false);

            ColorizeBackground(rightBg, SSytem.rightColor, false);
            ColorizeBackground(leftBg, SSytem.leftColor, false);
            ColorizeBackground(bothBg, Color.white, false);
        }
        private void EnableSelectionBoth()
        {
            Colorize(rightOutline, SSytem.rightColor, false);
            Colorize(leftOutline, SSytem.leftColor, false);
            Colorize(bothOutline, Color.white, true);

            Colorize(rightImage, SSytem.rightColor, false);
            Colorize(leftImage, SSytem.leftColor, false);
            Colorize(bothLeftImage, SSytem.leftColor, true);
            Colorize(bothRightImage, SSytem.rightColor, true);

            ColorizeBackground(rightBg, SSytem.rightColor, false);
            ColorizeBackground(leftBg, SSytem.leftColor, false);
            ColorizeBackground(bothBg, Color.white, true);
        }
        private void EnableSelectionRight()
        {
            Colorize(rightOutline, SSytem.rightColor, true);
            Colorize(leftOutline, SSytem.leftColor, false);
            Colorize(bothOutline, Color.white, false);

            Colorize(rightImage, SSytem.rightColor, true);
            Colorize(leftImage, SSytem.leftColor, false);
            Colorize(bothLeftImage, SSytem.leftColor, false);
            Colorize(bothRightImage, SSytem.rightColor, false);

            ColorizeBackground(rightBg, SSytem.rightColor, true);
            ColorizeBackground(leftBg, SSytem.leftColor, false);
            ColorizeBackground(bothBg, Color.white, false);
        }
        private void EnableSelectionLeft()
        {
            Colorize(rightOutline, SSytem.rightColor, false);
            Colorize(leftOutline, SSytem.leftColor, true);
            Colorize(bothOutline, Color.white, false);

            Colorize(rightImage, SSytem.rightColor, false);
            Colorize(leftImage, SSytem.leftColor, true);
            Colorize(bothLeftImage, SSytem.leftColor, false);
            Colorize(bothRightImage, SSytem.rightColor, false);

            ColorizeBackground(rightBg, SSytem.rightColor, false);
            ColorizeBackground(leftBg, SSytem.leftColor, true);
            ColorizeBackground(bothBg, Color.white, false);
        }


        private void Colorize(Image image, Color color, bool enabled)
        {
            image.color = new Color(color.r, color.g, color.b, color.a * (enabled ? 1 : 0.2f));
        }
        private void ColorizeBackground(Image image, Color color, bool enabled)
        {
            image.color = new Color(color.r, color.g, color.b, color.a * (enabled ? 0.2f : 0.02f));
        }
    }

    public enum SaberHand
    {
        Left = -1, 
        Both = 0, 
        Right = 1
    }
}
