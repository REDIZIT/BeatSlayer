using UnityEngine;

namespace InGame.UI.Menu.Wrappers
{
    public class MainScreenWrapper : BasicWrapper
    {
        public RectTransform buttonsParent;
        public RectTransform settingsBtn, leaderboardBtn;

        [Header("Positions")]
        public Vector2 buttonsHorizontal;
        public Vector2 buttonsVertical;
        public Vector2 anchoredPositionHorizontal, anchoredPositionVertical;

        public override void OnResolutionChange(bool isVertical)
        {
            buttonsParent.anchoredPosition = isVertical ? buttonsVertical : buttonsHorizontal;


            settingsBtn.anchoredPosition = isVertical ? anchoredPositionVertical : anchoredPositionHorizontal;


            // Reverse x axis
            leaderboardBtn.anchoredPosition = isVertical ? anchoredPositionVertical : anchoredPositionHorizontal;
            leaderboardBtn.anchoredPosition = new Vector2(-leaderboardBtn.anchoredPosition.x, leaderboardBtn.anchoredPosition.y);
        }
    }
}
