using InGame.Settings;
using InGame.UI.Menu.Wrappers;
using TMPro;
using UnityEngine;

namespace InGame.UI.Game
{
    public class MeshUIWrapper : BasicWrapper
    {
        public Camera cam;
        public Transform leftGroup, rightGroup;

        [Header("Positions")]
        public Vector3 leftGroupHorizontal;
        public Vector3 leftGroupVertical;

        [Header("Materials")]
        public Material overlayMaterial;

        private float defaultFOV;

        private void Start()
        {
            defaultFOV = cam.fieldOfView;

            if (SettingsManager.Settings.Gameplay.TextMeshOverlay)
            {
                SetOverlayMaterialForTexts();
            }
        }

        public override void OnResolutionChange(bool isVertical)
        {
            leftGroup.localPosition = isVertical ? leftGroupVertical : leftGroupHorizontal;

            rightGroup.localPosition = leftGroup.localPosition;
            rightGroup.localPosition = new Vector3(-rightGroup.localPosition.x, rightGroup.localPosition.y, rightGroup.localPosition.z);

            cam.fieldOfView = defaultFOV + (isVertical ? 20 : 0);
        }

        private void SetOverlayMaterialForTexts()
        {
            foreach (Transform go in leftGroup)
            {
                TextMeshPro text = go.GetComponent<TextMeshPro>();
                text.fontMaterial = overlayMaterial;
            }
            foreach (Transform go in rightGroup)
            {
                TextMeshPro text = go.GetComponent<TextMeshPro>();
                text.fontMaterial = overlayMaterial;
            }
        }
    }
}
