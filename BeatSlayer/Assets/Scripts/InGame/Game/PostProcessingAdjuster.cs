using InGame.Settings;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace InGame
{
    [RequireComponent(typeof(Volume))]
    public class PostProcessingAdjuster : MonoBehaviour
    {
        private Volume volume;
        private Camera cam;

        private void Start()
        {
            volume = GetComponent<Volume>();
            cam = Camera.main;

            ApplySettings();
        }

        private void ApplySettings()
        {
            var settings = SettingsManager.Settings.Graphics;

            Bloom bloom = (Bloom)volume.sharedProfile.components.Find(c => c is Bloom);
            bloom.active = settings.IsGlowEnabled;

            bloom.intensity.value = GetIntesity(settings.GlowPower);
            bloom.clamp.value = GetClamp(settings.GlowQuality);

            cam.allowHDR = settings.GlowQuality == GlowQuality.High;
        }

        private float GetIntesity(GlowPower power)
        {
            switch (power)
            {
                case GlowPower.High: return 3.2f;
                case GlowPower.Middle: return 2.1f;
                case GlowPower.Low: return 1.3f;
                default:
                    Debug.Log($"Can't find value for GlowPower = {power}. Default value (1) will be used.");
                    return 1;
            }
        }
        private float GetClamp(GlowQuality quality)
        {
            return (int)quality;
        }
    }
}