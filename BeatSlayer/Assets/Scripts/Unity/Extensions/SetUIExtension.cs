using UnityEngine.UI;

/*
 * Unity Forum link:
 * https://forum.unity.com/threads/change-the-value-of-a-toggle-without-triggering-onvaluechanged.275056/
*/


namespace Unity.Extensions
{
    public static class UIEventSyncExtensions
    {
        static Slider.SliderEvent emptySliderEvent = new Slider.SliderEvent();
        public static void SetValue(this Slider instance, float value)
        {
            var originalEvent = instance.onValueChanged;
            instance.onValueChanged = emptySliderEvent;
            instance.value = value;
            instance.onValueChanged = originalEvent;
        }



        static Toggle.ToggleEvent emptyToggleEvent = new Toggle.ToggleEvent();
        public static void SetValue(this Toggle instance, bool value)
        {
            var originalEvent = instance.onValueChanged;
            instance.onValueChanged = emptyToggleEvent;
            instance.isOn = value;
            instance.onValueChanged = originalEvent;
        }



        static InputField.OnChangeEvent emptyInputFieldEvent = new InputField.OnChangeEvent();
        public static void SetValue(this InputField instance, string value)
        {
            var originalEvent = instance.onValueChanged;
            instance.onValueChanged = emptyInputFieldEvent;
            instance.text = value;
            instance.onValueChanged = originalEvent;
        }
        public static void SetValue(this InputField instance, float value)
        {
            SetValue(instance, value.ToString());
        }

        // TODO: Add more UI types here.
    }
}