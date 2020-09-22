using InGame.Menu.Shop;
using UnityEngine;
using UnityEngine.UI;

namespace InGame.UI.Menu.Shop
{
    public class ShopColorSection : MonoBehaviour
    {
        public ShopHelper shop;
        public ColorPickerUI colorPicker;

        //public Image leftColorImg, rightColorImg, leftDirColorImg, rightArrowColorImg;
        public Image leftSaberColorImg, rightSaberColorImg;
        public Image leftCubeColorImg, rightCubeColorImg;

        public Slider glowSaberLeftSlider, glowSaberRightSlider, glowCubeLeftSlider, glowCubeRightSlider;
        public Slider trailLengthSlider;

        private int colorpickingForId;



        private void Start()
        {
            Refresh();
        }



        public void Refresh()
        {
            leftSaberColorImg.color = SSytem.leftColor;
            rightSaberColorImg.color = SSytem.rightColor;

            leftCubeColorImg.color = SSytem.leftDirColor;
            rightCubeColorImg.color = SSytem.rightDirColor;

            RefreshSliders();
        }


        public void OnGlowPowerSliderChange()
        {
            SSytem.GlowPowerSaberLeft = (int)glowSaberLeftSlider.value;
            SSytem.GlowPowerSaberRight = (int)glowSaberRightSlider.value;
            SSytem.GlowPowerCubeLeft = (int)glowCubeLeftSlider.value;
            SSytem.GlowPowerCubeRight = (int)glowCubeRightSlider.value;
            SSytem.TrailLength = (int)trailLengthSlider.value;

            SSytem.SaveFile();
        }
        public async void OnSaberColorBtnClick(int hand)
        {
            Color color = await colorPicker.GetColorAsync(hand == -1 ? SSytem.leftColor : SSytem.rightColor);

            if (hand == -1) SSytem.leftColor = color;
            else if (hand == 1) SSytem.rightColor = color;

            SSytem.SaveFile();

            Refresh();
            shop.FillSabersView();
        }
        public async void OnCubeColorBtnClick(int hand)
        {
            Color color = await colorPicker.GetColorAsync(hand == -1 ? SSytem.leftDirColor : SSytem.rightDirColor);

            if (hand == -1) SSytem.leftDirColor = color;
            else if (hand == 1) SSytem.rightDirColor = color;

            SSytem.SaveFile();

            Refresh();
            shop.FillSabersView();
        }

        public void OnColorpicked(Color clr)
        {
            if (colorpickingForId == -1) SSytem.leftColor = clr;
            if (colorpickingForId == 1) SSytem.rightColor = clr;
        }

        private void RefreshSliders()
        {
            glowSaberLeftSlider.SetValueWithoutNotify(SSytem.GlowPowerSaberLeft);
            glowSaberRightSlider.SetValueWithoutNotify(SSytem.GlowPowerSaberRight);
            glowCubeLeftSlider.SetValueWithoutNotify(SSytem.GlowPowerCubeLeft);
            glowCubeRightSlider.SetValueWithoutNotify(SSytem.GlowPowerCubeRight);
            trailLengthSlider.SetValueWithoutNotify(SSytem.TrailLength);
        }
    }
}
