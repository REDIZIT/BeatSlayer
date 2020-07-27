using InGame.UI.Menu.Wrappers;
using UnityEngine;

namespace InGame.UI.Game
{
    public class FinishWrapper : BasicWrapper
    {
        public RectTransform leaderboard/*,*/ /*scoring,*/ /*difficulties*/;

        [Header("Anchors")]
        public Vector4 leaderboardHorizontal;
        public Vector4 leaderboardVertical;
        //public Vector4 scoringHorizontal, scoringVertical;
        //public Vector4 diffsHorizontal, diffsVertical;

        public override void OnResolutionChange(bool isVertical)
        {
            FitAnchors(leaderboard, leaderboardHorizontal, leaderboardVertical, isVertical);
            //FitAnchors(scoring, scoringHorizontal, scoringVertical, isVertical);
            //FitAnchors(difficulties, diffsHorizontal, diffsVertical, isVertical);
        }

        private void FitAnchors(RectTransform rect, Vector4 horizontal, Vector4 vertical, bool isVertical)
        {
            rect.anchorMin = GetVector2(isVertical ? vertical : horizontal, false);
            rect.anchorMax = GetVector2(isVertical ? vertical : horizontal, true);
        }
        private Vector2 GetVector2(Vector4 v4, bool takeMaxPart)
        {
            return takeMaxPart ? new Vector2(v4.z, v4.w) : new Vector2(v4.x, v4.y);
        }
    }
}
