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
            RefreshSections(isBought/*prefs.boughtSabers[item.id]*/);

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





        private void RefreshSections(bool isBought)
        {
            buySection.SetActive(!isBought);
            selectSection.SetActive(isBought);

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
            Colorize(rightOutline, SSytem.instance.rightColor, false);
            Colorize(leftOutline, SSytem.instance.leftColor, false);
            Colorize(bothOutline, Color.white, false);

            Colorize(rightImage, SSytem.instance.rightColor, false);
            Colorize(leftImage, SSytem.instance.leftColor, false);
            Colorize(bothLeftImage, SSytem.instance.leftColor, false);
            Colorize(bothRightImage, SSytem.instance.rightColor, false);

            ColorizeBackground(rightBg, SSytem.instance.rightColor, false);
            ColorizeBackground(leftBg, SSytem.instance.leftColor, false);
            ColorizeBackground(bothBg, Color.white, false);
        }
        private void EnableSelectionBoth()
        {
            Colorize(rightOutline, SSytem.instance.rightColor, false);
            Colorize(leftOutline, SSytem.instance.leftColor, false);
            Colorize(bothOutline, Color.white, true);

            Colorize(rightImage, SSytem.instance.rightColor, false);
            Colorize(leftImage, SSytem.instance.leftColor, false);
            Colorize(bothLeftImage, SSytem.instance.leftColor, true);
            Colorize(bothRightImage, SSytem.instance.rightColor, true);

            ColorizeBackground(rightBg, SSytem.instance.rightColor, false);
            ColorizeBackground(leftBg, SSytem.instance.leftColor, false);
            ColorizeBackground(bothBg, Color.white, true);
        }
        private void EnableSelectionRight()
        {
            Colorize(rightOutline, SSytem.instance.rightColor, true);
            Colorize(leftOutline, SSytem.instance.leftColor, false);
            Colorize(bothOutline, Color.white, false);

            Colorize(rightImage, SSytem.instance.rightColor, true);
            Colorize(leftImage, SSytem.instance.leftColor, false);
            Colorize(bothLeftImage, SSytem.instance.leftColor, false);
            Colorize(bothRightImage, SSytem.instance.rightColor, false);

            ColorizeBackground(rightBg, SSytem.instance.rightColor, true);
            ColorizeBackground(leftBg, SSytem.instance.leftColor, false);
            ColorizeBackground(bothBg, Color.white, false);
        }
        private void EnableSelectionLeft()
        {
            Colorize(rightOutline, SSytem.instance.rightColor, false);
            Colorize(leftOutline, SSytem.instance.leftColor, true);
            Colorize(bothOutline, Color.white, false);

            Colorize(rightImage, SSytem.instance.rightColor, false);
            Colorize(leftImage, SSytem.instance.leftColor, true);
            Colorize(bothLeftImage, SSytem.instance.leftColor, false);
            Colorize(bothRightImage, SSytem.instance.rightColor, false);

            ColorizeBackground(rightBg, SSytem.instance.rightColor, false);
            ColorizeBackground(leftBg, SSytem.instance.leftColor, true);
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
