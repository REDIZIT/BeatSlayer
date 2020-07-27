using UnityEngine;

namespace InGame.UI.Menu.Wrappers
{
    public class BeatmapWrapper : BasicWrapper
    {
        public RectTransform mapSection, difficultySection, gameSection;


        [Header("Anchors")]
        [Tooltip("MinX, MinY, MaxX, MaxY")]
        public Vector4 anchorsMapHorizontal;
        public Vector4 anchorsMapVertical;

        public Vector4 anchorsDiffHorizontal, anchorsDiffVertical;
        public Vector4 anchorsGameHorizontal, anchorsGameVertical;
        
        public override void OnResolutionChange(bool isVertical)
        {
            mapSection.anchorMin = GetVector2(isVertical ? anchorsMapVertical : anchorsMapHorizontal, false);
            mapSection.anchorMax = GetVector2(isVertical ? anchorsMapVertical : anchorsMapHorizontal, true);

            difficultySection.anchorMin= GetVector2(isVertical ? anchorsDiffVertical : anchorsDiffHorizontal, false);
            difficultySection.anchorMax = GetVector2(isVertical ? anchorsDiffVertical : anchorsDiffHorizontal, true);

            gameSection.anchorMin = GetVector2(isVertical ? anchorsGameVertical : anchorsGameHorizontal, false);
            gameSection.anchorMax = GetVector2(isVertical ? anchorsGameVertical : anchorsGameHorizontal, true);
        }

        private Vector2 GetVector2(Vector4 v4, bool takeMaxPart)
        {
            return takeMaxPart ? new Vector2(v4.z, v4.w) : new Vector2(v4.x, v4.y);
        }
    }
}
