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
        public bool IsOn { get; private set; }
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
                    if (IsOn == true)
                    {
                        switchAnimator.Play("Switch On");
                        IsOn = true;
                        PlayerPrefs.SetString(switchTag + "Switch", "true");
                    }

                    else
                    {
                        switchAnimator.Play("Switch Off");
                        IsOn = false;
                        PlayerPrefs.SetString(switchTag + "Switch", "false");
                    }
                }

                else if (PlayerPrefs.GetString(switchTag + "Switch") == "true")
                {
                    switchAnimator.Play("Switch On");
                    IsOn = true;
                }

                else if (PlayerPrefs.GetString(switchTag + "Switch") == "false")
                {
                    switchAnimator.Play("Switch Off");
                    IsOn = false;
                }
            }

            else
            {
                if (IsOn == true)
                {
                    switchAnimator.Play("Switch On");
                    IsOn = true;
                }

                else
                {
                    switchAnimator.Play("Switch Off");
                    IsOn = false;
                }
            }

            if (invokeAtStart == true && IsOn == true)
                OnEvents.Invoke();
            if (invokeAtStart == true && IsOn == false)
                OffEvents.Invoke();
        }

        public void AnimateSwitch()
        {
            if (IsOn == true)
            {
                SetOff(false);
            }
            else
            {
                SetOn(false);
            }

            OnValueChange?.Invoke(IsOn);
        }
        private void SetOff(bool force)
        {
            switchAnimator.Play(force ? "ForceSwitchOff" : "Switch Off");
            IsOn = false;
            OffEvents.Invoke();

            if (saveValue == true)
                PlayerPrefs.SetString(switchTag + "Switch", "false");
        }
        private void SetOn(bool force)
        {
            switchAnimator.Play(force ? "ForceSwitchOn" : "Switch On");
            IsOn = true;
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

            OnValueChange?.Invoke(IsOn);
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