using Assets.SimpleLocalization;
using Michsky.UI.ModernUIPack;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace InGame.Menu.Settings
{
    public class SettingsMediaViewer : MonoBehaviour
    {
        public static SettingsMediaViewer instance;

        public GameObject overlay;
        public Image mediaImage;
        public HorizontalSelector selector;


        public Sprite[] sprites;

        [Header("All sprites")]
        public SettingsMedia[] allMedia;


        private void Awake()
        {
            instance = this;
            selector.OnValueChanged += OnSelectorIndexChange;
        }
        
        public void Show(string[] names)
        {
            overlay.SetActive(true);
            SettingsMedia[] media = allMedia.Where(c => names.Any(n => n == c.label)).ToArray();
            Show(media);
        }
        public void Show(SettingsMedia[] media)
        {
            sprites = media.Select(c => c.sprite).ToArray();
            mediaImage.sprite = sprites[0];

            selector.itemList.Clear();
            foreach (string label in media.Select(c => c.label))
            {
                selector.itemList.Add(new HorizontalSelector.Item()
                {
                    itemTitle = LocalizationManager.Localize(label)
                });
            }

            selector.index = 0;
            selector.RefreshTitle();
        }

        private void OnSelectorIndexChange(int index)
        {
            mediaImage.sprite = sprites[index];
        }
    }

    [Serializable]
    public class SettingsMedia
    {
        public string label;
        public Sprite sprite;

        public SettingsMedia(string label, Sprite sprite)
        {
            this.label = label;
            this.sprite = sprite;
        }
    }
}
