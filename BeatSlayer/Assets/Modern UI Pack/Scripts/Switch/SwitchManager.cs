using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System;

namespace Michsky.UI.ModernUIPack
{
    public class SwitchManager : MonoBehaviour
    {
        [Header("SETTINGS")]
        [Tooltip("IMPORTANT! EVERY SWITCH MUST HAVE A DIFFERENT TAG")]
        public string switchTag = "Switch";
        public bool isOn;
        public bool saveValue = true;
        public bool invokeAtStart = true;

        public UnityEvent OnEvents;
        public UnityEvent OffEvents;

        public Action<bool> OnValueChange { get; set; }

        Animator switchAnimator;
        Button switchButton;

        void Awake()
        {
            switchAnimator = gameObject.GetComponent<Animator>();
            switchButton = gameObject.GetComponent<Button>();
            switchButton.onClick.AddListener(AnimateSwitch);

            if (saveValue == true)
            {
                if (PlayerPrefs.GetString(switchTag + "Switch") == "")
                {
                    if (isOn == true)
                    {
                        switchAnimator.Play("Switch On");
                        isOn = true;
                        PlayerPrefs.SetString(switchTag + "Switch", "true");
                    }

                    else
                    {
                        switchAnimator.Play("Switch Off");
                        isOn = false;
                        PlayerPrefs.SetString(switchTag + "Switch", "false");
                    }
                }

                else if (PlayerPrefs.GetString(switchTag + "Switch") == "true")
                {
                    switchAnimator.Play("Switch On");
                    isOn = true;
                }

                else if (PlayerPrefs.GetString(switchTag + "Switch") == "false")
                {
                    switchAnimator.Play("Switch Off");
                    isOn = false;
                }
            }

            else
            {
                if (isOn == true)
                {
                    switchAnimator.Play("Switch On");
                    isOn = true;
                }

                else
                {
                    switchAnimator.Play("Switch Off");
                    isOn = false;
                }
            }

            if (invokeAtStart == true && isOn == true)
                OnEvents.Invoke();
            if (invokeAtStart == true && isOn == false)
                OffEvents.Invoke();
        }

        public void AnimateSwitch()
        {
            if (isOn == true)
            {
                SetOff(false);
            }
            else
            {
                SetOn(false);
            }

            OnValueChange?.Invoke(isOn);
        }
        private void SetOff(bool force)
        {
            switchAnimator?.Play(force ? "ForceSwitchOff" : "Switch Off");
            isOn = false;
            OffEvents.Invoke();

            if (saveValue == true)
                PlayerPrefs.SetString(switchTag + "Switch", "false");
        }
        private void SetOn(bool force)
        {
            switchAnimator?.Play(force ? "ForceSwitchOn" : "Switch On");
            isOn = true;
            OnEvents.Invoke();

            if (saveValue == true)
                PlayerPrefs.SetString(switchTag + "Switch", "true");
        }

        public void SetValue(bool isOn, bool force = false)
        {
            //if(force && switchAnimator.GetCurrentAnimatorStateInfo(0).IsName("Switch On") || switchAnimator.GetCurrentAnimatorStateInfo(0).IsName("Switch Off"))
            //{
            //    return;
            //}

            if (isOn) SetOff(force);
            else SetOn(force);

            OnValueChange?.Invoke(this.isOn);
        }
        public void SetValueWithoutNotify(bool isOn, bool force = false)
        {
            //if (force && switchAnimator.GetCurrentAnimatorStateInfo(0).IsName("Switch On") || switchAnimator.GetCurrentAnimatorStateInfo(0).IsName("Switch Off"))
            //{
            //    return;
            //}

            if (isOn) SetOn(force);
            else SetOff(force);
        }
    }
}