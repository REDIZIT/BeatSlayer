using UnityEngine;
using UnityEngine.UI;

namespace InGame.UI.Menu.Sidebar
{
    public class SidebarScaler : MonoBehaviour
    {
        public SmartGridLayout group;

        public enum Orientation
        {
            Vertical, Horizonal
        }

        private Orientation itemsOrientation;
        public Orientation ItemsOrientation
        {
            get
            {
                return itemsOrientation;
            }
            set
            {
                if (itemsOrientation != value)
                {
                    itemsOrientation = value;
                    Build();
                }
            }
        }



        public float height, width;

        private RectTransform rect;

        private void Awake()
        {
            rect = GetComponent<RectTransform>();
            Build();
        }

        public void Build()
        {
            if (itemsOrientation == Orientation.Vertical)
            {
                rect.anchorMin = new Vector2(0, 0);
                rect.anchorMax = new Vector2(0, 1);

                rect.offsetMin = new Vector2(-width, 0);
                rect.offsetMax = new Vector2(0, 0);

                rect.anchoredPosition = new Vector2(0, 0);
            }
            else
            {
                rect.anchorMin = new Vector2(0, 1);
                rect.anchorMax = new Vector2(1, 1);

                rect.offsetMin = new Vector2(0, 0);
                rect.offsetMax = new Vector2(0, height);

                rect.anchoredPosition = new Vector2(0, -height);
            }
        }
    }
}
