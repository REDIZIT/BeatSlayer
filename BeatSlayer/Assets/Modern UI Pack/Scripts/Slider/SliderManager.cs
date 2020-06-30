using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System;
using UnityEngine.UI.Michsky.UI.ModernUIPack;

namespace Michsky.UI.ModernUIPack
{
    public class SliderManager : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("TEXTS")]
        public TextMeshProUGUI valueText;
        public TextMeshProUGUI popupValueText;

        [Header("SETTINGS")]
        public bool usePercent = false;
        public bool showValue = true;
        public bool showPopupValue = true;
        public bool useRoundValue = false;
        public bool dynamicOffset = false;
        public bool colorizeHandle = false;
        public Image handle;

        public Func<float, string> Format;

        [Header("GRADIENTS")]
        public UIGradient fillGradient;

        public enum FormatType { None, Time }
        public FormatType type;

        private Slider mainSlider;
        private Animator sliderAnimator;

        void Start()
        {
            mainSlider = this.GetComponent<Slider>();
            sliderAnimator = this.GetComponent<Animator>();

            if (showValue == false && valueText != null)
                valueText.enabled = false;

            if (showPopupValue == false && popupValueText != null)
                popupValueText.enabled = false;

            if(type == FormatType.Time)
            {
                Format = (f) =>
                {
                    int minutes = Mathf.FloorToInt(f / 60f);
                    int seconds = Mathf.FloorToInt(f - minutes * 60);

                    return minutes + ":" + (seconds < 10 ? "0" + seconds : "" + seconds);
                };
            }
        }

        void Update()
        {
            if (dynamicOffset && fillGradient != null && mainSlider != null)
            {
                fillGradient.Offset = (mainSlider.maxValue - mainSlider.value) / mainSlider.maxValue;
            }


            if (colorizeHandle && handle != null && fillGradient != null)
            {
                handle.color = fillGradient.EffectGradient.Evaluate(mainSlider.value / mainSlider.maxValue) * 0.95f;
            }


            if (valueText == null || popupValueText == null) return;

            if (useRoundValue == true)
            {
                if (usePercent == true)
                {
                    valueText.text = Mathf.Round(mainSlider.value * 1.0f).ToString() + "%";
                    popupValueText.text = Mathf.Round(mainSlider.value * 1.0f).ToString() + "%";
                }

                else
                {
                    if(Format != null)
                    {
                        string str = Format(mainSlider.value * 1.0f);
                        valueText.text = str;
                        popupValueText.text = str;
                    }
                    else
                    {
                        valueText.text = Mathf.Round(mainSlider.value * 1.0f).ToString();
                        popupValueText.text = Mathf.Round(mainSlider.value * 1.0f).ToString();
                    }
                }
            }

            else
            {
                if (usePercent == true)
                {
                    valueText.text = mainSlider.value.ToString("F1") + "%";
                    popupValueText.text = mainSlider.value.ToString("F1") + "%";
                }

                else
                {
                    if (Format != null)
                    {
                        string str = Format(mainSlider.value);
                        valueText.text = str;
                        popupValueText.text = str;
                    }
                    else
                    {
                        valueText.text = mainSlider.value.ToString("F1");
                        popupValueText.text = mainSlider.value.ToString("F1");
                    }
                }
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (showPopupValue == true)
                sliderAnimator.Play("Value In");
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (showPopupValue == true)
                sliderAnimator.Play("Value Out");
        }
    }
}