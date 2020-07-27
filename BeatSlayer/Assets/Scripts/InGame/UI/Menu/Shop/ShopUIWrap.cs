using InGame.UI.Menu.Sidebar;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace InGame.UI.Menu.Shop
{
    public class ShopUIWrap : MonoBehaviour
    {
        public SidebarScaler sidebar;
        public RectTransform body;

        public ScrollRect sabersScrollRect;
        public GridLayoutGroup sabersLayoutGroup;
        public ContentSizeFitter sabersFitter;

        private Vector2 mappedResolution;


        private void Start()
        {
            Build();
        }
        private void Update()
        {
            if(Screen.width != mappedResolution.x || Screen.height != mappedResolution.y)
            {
                Build();
            }
        }

        public void Build()
        {
            StartCoroutine(IEBuild());
        }
        private IEnumerator IEBuild()
        {
            mappedResolution = new Vector2(Screen.width, Screen.height);

            bool isVerticalMode = Screen.height > Screen.width;

            sidebar.ItemsOrientation = isVerticalMode ? SidebarScaler.Orientation.Horizonal : SidebarScaler.Orientation.Vertical;

            sabersScrollRect.vertical = isVerticalMode;
            sabersScrollRect.horizontal = !isVerticalMode;

            // Content fitting
            //sabersFitter.verticalFit = isVerticalMode ? ContentSizeFitter.FitMode.PreferredSize : ContentSizeFitter.FitMode.Unconstrained;
            //sabersFitter.horizontalFit = !isVerticalMode ? ContentSizeFitter.FitMode.PreferredSize : ContentSizeFitter.FitMode.Unconstrained;

            //sabersFitter.enabled = false;


            sabersLayoutGroup.constraint = isVerticalMode ? GridLayoutGroup.Constraint.FixedColumnCount : GridLayoutGroup.Constraint.FixedRowCount;


            // Waiting for preferredHeight and width will be calculated
            yield return null;

            //sabersFitter.enabled = true;

            body.offsetMin = new Vector2(isVerticalMode ? 0 : 400, 0);
            body.offsetMax = new Vector2(0, isVerticalMode ? -sidebar.group.preferredHeight - 50 : 0);
        }
    }
}
