using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace InGame.Menu.Shop
{
    public class ColorPickerUI : MonoBehaviour
    {
        public GameObject overlay;

        public Image preview;

        public Slider redSlider, greenSlider, blueSlider;
        public TMP_InputField redField, greenField, blueField;

        private bool isColorSelected;


        public async Task<Color> GetColorAsync(Color startColor)
        {
            Open(startColor);

            return await Task.Run(async () =>
            {
                while (!isColorSelected)
                {
                    await Task.Delay(16);
                }

                return preview.color;
            });
        }

        private void Open(Color startColor)
        {
            isColorSelected = false;
            overlay.SetActive(true);

            SetColor(startColor);
        }
        public void OnSelectButtonClick()
        {
            isColorSelected = true;
            overlay.SetActive(false);
        }


        public void RefreshPreview()
        {
            int r = (int)redSlider.value;
            int g = (int)greenSlider.value;
            int b = (int)blueSlider.value;

            preview.color = new Color(r / 255f, g / 255f, b / 255f);
        }

        public void OnSliderValueChange()
        {
            RefreshFields();
            RefreshPreview();
        }
        public void OnFieldValueChanged()
        {
            RefreshSliders();
            RefreshPreview();
        }
        public void OnPresetBtnClick(Image copyFromImage)
        {
            SetColor(copyFromImage.color);
        }








        private void RefreshFields()
        {
            redField.SetTextWithoutNotify(redSlider.value + "");
            greenField.SetTextWithoutNotify(greenSlider.value + "");
            blueField.SetTextWithoutNotify(blueSlider.value + "");
        }
        private void RefreshSliders()
        {
            RefreshSlider(redSlider, redField.text);
            RefreshSlider(greenSlider, greenField.text);
            RefreshSlider(blueSlider, blueField.text);
        }
        private void RefreshSlider(Slider slider, string text)
        {
            if (int.TryParse(text, out int value))
            {
                slider.SetValueWithoutNotify(value);
            }
        }
        private void SetColor(Color color)
        {
            redSlider.SetValueWithoutNotify(color.r * 255);
            greenSlider.SetValueWithoutNotify(color.g * 255);
            blueSlider.SetValueWithoutNotify(color.b * 255);

            OnSliderValueChange();
        }
    }
}
