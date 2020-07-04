using System;
using UnityEngine;

namespace InGame.Settings
{

    public class OptionAttribute : Attribute
    {
        public string DisplayName { get; set; }
        public string Description { get; set; }

        public OptionAttribute(string DisplayName)
        {
            this.DisplayName = DisplayName;
        }

        public OptionAttribute(string DisplayName, string Description)
        {
            this.DisplayName = DisplayName;
            this.Description = Description;
        }
    }

    public class RangeAttribute : Attribute
    {
        public float MinValue { get; set; }
        public float MaxValue { get; set; }

        public RangeAttribute(float MinValue, float MaxValue)
        {
            this.MinValue = MinValue;
            this.MaxValue = MaxValue;
        }

        public float Clamp(float value)
        {
            Debug.Log($"Clamp {value} with min {MinValue} and max {MaxValue} = {Mathf.Clamp(value, MinValue, MaxValue)}");
            return Mathf.Clamp(value, MinValue, MaxValue);
        }
    }
    public class EnabledAttribute : Attribute
    {
        public string BaseName { get; set; }

        public EnabledAttribute(string BaseName)
        {
            this.BaseName = BaseName;
        }
    }
}
