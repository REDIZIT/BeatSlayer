using UnityEngine;

namespace InGame.UI.Menu.Wrappers
{
    /// <summary>
    /// Class you need to inherit from for using OnResolutionChange event
    /// </summary>
    public abstract class BasicWrapper : MonoBehaviour
    {
        private Vector2 layoutedResolution;

        /// <summary>
        /// Update only for checking on change resolution event. Without it wrapper is useless :D
        /// </summary>
        private void Update()
        {
            bool changeEvent;

            changeEvent = Screen.width != layoutedResolution.x || Screen.height != layoutedResolution.y;

            if (!Application.isEditor)
            {
                if (!DeviceAutoRotationIsOn() && layoutedResolution != Vector2.zero)
                {
                    changeEvent = false;
                }
            }

            bool isUnlocked = DeviceAutoRotationIsOn();

            // If auto rotation is locked, then set false any rotation to
            // prevent Unity from rotation coz if can't make if itself :/
            Screen.autorotateToLandscapeLeft = isUnlocked;
            Screen.autorotateToLandscapeRight = isUnlocked;
            Screen.autorotateToPortrait = isUnlocked;
            Screen.autorotateToPortraitUpsideDown = isUnlocked;


            if (changeEvent)
            {
                layoutedResolution = new Vector2(Screen.width, Screen.height);

                OnResolutionChange(Screen.height > Screen.width);
            }
        }



        /// <summary>
        /// Is screen auto rotation is unlocked by android
        /// </summary>
        /// <returns></returns>
        public static bool DeviceAutoRotationIsOn()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
    using (var actClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        {
            var context = actClass.GetStatic<AndroidJavaObject>("currentActivity");
            AndroidJavaClass systemGlobal = new AndroidJavaClass("android.provider.Settings$System");
            var rotationOn = systemGlobal.CallStatic<int>("getInt", context.Call<AndroidJavaObject>("getContentResolver"), "accelerometer_rotation");
 
            return rotationOn==1;
        }
#endif
            return true;
        }

        /// <summary>
        /// Invoked from Update()
        /// </summary>
        public abstract void OnResolutionChange(bool isVertical);
    }
}
