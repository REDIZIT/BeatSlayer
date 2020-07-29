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
            leftSaberColorImg.color = SSytem.instance.leftColor;
            rightSaberColorImg.color = SSytem.instance.rightColor;

            leftCubeColorImg.color = SSytem.instance.leftDirColor;
            rightCubeColorImg.color = SSytem.instance.rightDirColor;

            RefreshSliders();
        }


        public void OnGlowPowerSliderChange()
        {
            SSytem.instance.GlowPowerSaberLeft = (int)glowSaberLeftSlider.value;
            SSytem.instance.GlowPowerSaberRight = (int)glowSaberRightSlider.value;
            SSytem.instance.GlowPowerCubeLeft = (int)glowCubeLeftSlider.value;
            SSytem.instance.GlowPowerCubeRight = (int)glowCubeRightSlider.value;
            SSytem.instance.TrailLength = (int)trailLengthSlider.value;

            SSytem.instance.SaveFile();
        }
        public async void OnSaberColorBtnClick(int hand)
        {
            Color color = await colorPicker.GetColorAsync(hand == -1 ? SSytem.instance.leftColor : SSytem.instance.rightColor);

            if (hand == -1) SSytem.instance.leftColor = color;
            else if (hand == 1) SSytem.instance.rightColor = color;

            SSytem.instance.SaveFile();

            Refresh();
            shop.FillSabersView();
        }
        public async void OnCubeColorBtnClick(int hand)
        {
            Color color = await colorPicker.GetColorAsync(hand == -1 ? SSytem.instance.leftDirColor : SSytem.instance.rightDirColor);

            if (hand == -1) SSytem.instance.leftDirColor = color;
            else if (hand == 1) SSytem.instance.rightDirColor = color;

            SSytem.instance.SaveFile();

            Refresh();
            shop.FillSabersView();
        }

        public void OnColorpicked(Color clr)
        {
            if (colorpickingForId == -1) SSytem.instance.leftColor = clr;
            if (colorpickingForId == 1) SSytem.instance.rightColor = clr;
        }

        private void RefreshSliders()
        {
            glowSaberLeftSlider.SetValueWithoutNotify(SSytem.instance.GlowPowerSaberLeft);
            glowSaberRightSlider.SetValueWithoutNotify(SSytem.instance.GlowPowerSaberRight);
            glowCubeLeftSlider.SetValueWithoutNotify(SSytem.instance.GlowPowerCubeLeft);
            glowCubeRightSlider.SetValueWithoutNotify(SSytem.instance.GlowPowerCubeRight);
            trailLengthSlider.SetValueWithoutNotify(SSytem.instance.TrailLength);
        }
    }
}
