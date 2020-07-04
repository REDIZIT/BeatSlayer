using Assets.SimpleLocalization;
using InGame.Game.Menu;
using UnityEngine;
using UnityEngine.UI;

namespace InGame.Settings
{
    public class SettingsUIGroup : SettingsUIItem
    {
        public Transform content;
        public GameObject itemPrefab;
        public Image image;

        public float declaratedRectHeight;


        private VerticalLayoutGroup group;



        public override void Refresh(SettingsUIItemModel model, SettingsUI ui, Transform dropdownlocker)
        {
            this.model = model;
            this.ui = ui;
            nameText.text = LocalizationManager.Localize(model.NameWithoutLocalization);



            RectTransform rect = GetComponent<RectTransform>();
            group = content.GetComponent<VerticalLayoutGroup>();

            rect.sizeDelta = new Vector2(rect.sizeDelta.x, group.preferredHeight);

            //StartCoroutine(IERefresh());
        }
        public void SetImage(SettingsOptionImage groupimage)
        {
            image.sprite = groupimage.sprite;
        }

        public override float GetHeight()
        {
             int itemsCount = content.childCount;

            float itemsHeights = 0;
            foreach (Transform child in content)
            {
                var item = child.GetComponent<SettingsUIItem>();
                itemsHeights += item.GetHeight();
            }


            float itemsSpacing = group.spacing * (itemsCount - 1) + group.padding.top + group.padding.bottom;
            itemsSpacing = itemsSpacing < 0 ? 0 : itemsSpacing;


            float height = declaratedRectHeight + itemsHeights + itemsSpacing;

            return height;
        }

        public void FitContent()
        {
            float height = GetHeight();

            RectTransform rect = GetComponent<RectTransform>();

            rect.sizeDelta = new Vector2(rect.sizeDelta.x, height);
        }
        //public IEnumerator IERefresh()
        //{
        //    //yield return null;

        //    VerticalLayoutGroup layout = ui.content.GetComponent<VerticalLayoutGroup>();

        //    layout.enabled = false;

        //    //yield return null;

        //    layout.enabled = true;

        //    yield return null;
        //}
    }
}
