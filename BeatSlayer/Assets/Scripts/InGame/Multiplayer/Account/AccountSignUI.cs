using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Assets.SimpleLocalization;
using InGame.Helpers;
using Multiplayer.Accounts;
using UnityEngine;
using UnityEngine.UI;

namespace BeatSlayerServer.Multiplayer.Accounts
{
    public class AccountSignUI : MonoBehaviour
    {
        public AccountUI ui;
        public GameObject window;
        
        public Text headerText;

        public InputField nickField, passwordField, password2Field, emailField;
        public RectTransform bodyRect;
        public VerticalLayoutGroup bodyLayoutGroup, windowLayoutGroup;

        public Color defaultColor, invalidColor;

        public Image eyeImage;
        public Sprite showSprite, hideSprite;

        public Button signUpBtn, logInBtn;

        private bool canContinue;
        private bool isLogInActive = true;

        [Header("Restore UI")] public GameObject restore_window;
        public InputField restore_nick;

        public InputField restore_newPassword, restore_code;
        public GameObject restore_resultPan;
        public Text restore_resultText;

        
        public void ShowLogIn()
        {
            window.SetActive(true);
            headerText.text = LocalizationManager.Localize("LogInAccount");
            isLogInActive = true;

            HelperUI.ColorInputField(nickField, defaultColor);
            HelperUI.ColorInputField(passwordField, defaultColor);
            HelperUI.ColorInputField(password2Field, defaultColor);
            HelperUI.ColorInputField(emailField, defaultColor);
            
            password2Field.gameObject.SetActive(false);
            emailField.gameObject.SetActive(false);

            signUpBtn.interactable = true;
            logInBtn.interactable = true;

            bodyRect.sizeDelta = new Vector2(bodyRect.sizeDelta.x, 50 + 3 * 68 + 2 * 30 + 50);
        }

        public void ShowSignUp()
        {
            window.SetActive(true);
            headerText.text = LocalizationManager.Localize("SignUpAccount");
            isLogInActive = false;

            HelperUI.ColorInputField(nickField, defaultColor);
            HelperUI.ColorInputField(passwordField, defaultColor);
            HelperUI.ColorInputField(password2Field, defaultColor);
            HelperUI.ColorInputField(emailField, defaultColor);
            
            password2Field.gameObject.SetActive(true);
            emailField.gameObject.SetActive(true);
            
            signUpBtn.interactable = true;
            logInBtn.interactable = true;

            bodyRect.sizeDelta = new Vector2(bodyRect.sizeDelta.x, 50 + 5 * 68 + 4 * 30 + 50);
        }



        public void OnInputFieldChange()
        {
            canContinue = true;
            
            if(nickField.text.Trim() == "") { HelperUI.ColorInputField(nickField, invalidColor); canContinue = false; }
            else { HelperUI.ColorInputField(nickField, defaultColor); return; }
            
            if(passwordField.text.Trim() == "") {HelperUI.ColorInputField(passwordField, invalidColor); canContinue = false; } 
            else HelperUI.ColorInputField(passwordField, defaultColor);
            
            if(password2Field.gameObject.activeSelf && password2Field.text != passwordField.text) { HelperUI.ColorInputField(password2Field, invalidColor); canContinue = false; } 
            else HelperUI.ColorInputField(password2Field, defaultColor);
            
            if(emailField.text != "" && emailField.text.Contains("@") && emailField.text.Contains(".")) { HelperUI.ColorInputField(emailField, defaultColor);}
            else if (emailField.text != "") { HelperUI.ColorInputField(emailField, invalidColor); canContinue = false; }
        }

        public void OnLogInBtnClick()
        {
            if (!isLogInActive)
            {
                ShowLogIn();
                return;
            }
            if (!canContinue) return;
            
            signUpBtn.interactable = false;
            logInBtn.interactable = false;
            ui.LogIn(nickField.text, passwordField.text);
        }

        public void OnSignUpBtnClick()
        {
            if (isLogInActive)
            {
                ShowSignUp();
                return;
            }
            if (!canContinue) return;

            signUpBtn.interactable = false;
            logInBtn.interactable = false;

            ui.SignUp(nickField.text, passwordField.text, RegionInfo.CurrentRegion.DisplayName, emailField.text);
        }



        public void OnLogInResult(OperationMessage op)
        {
            Debug.Log("OnLogInResult: " + op.Type.ToString() + " " + op.Message);
            signUpBtn.interactable = true;
            logInBtn.interactable = true;
            Debug.Log(signUpBtn.interactable);
            
            HelperUI.ColorInputField(nickField, defaultColor);   
            HelperUI.ColorInputField(passwordField, defaultColor);   
            
            if (op.Type == OperationMessage.OperationType.Success)
            {
                window.SetActive(false);
            }
            else
            {
                if (op.Message.ToLower().Contains("password"))
                {
                    HelperUI.ColorInputField(passwordField, invalidColor);
                    ui.ShowMessage(LocalizationManager.Localize("InvalidPassword"));
                }
                else
                {
                    HelperUI.ColorInputField(nickField, invalidColor);
                    ui.ShowMessage(LocalizationManager.Localize("NoSuchAccount"));
                }
            }
        }
        public void OnSignUpResult(OperationMessage op)
        {
            signUpBtn.interactable = true;
            logInBtn.interactable = true;
            if (op.Type == OperationMessage.OperationType.Success)
            {
                window.SetActive(false);
            }
            else
            {
                HelperUI.ColorInputField(nickField, invalidColor);
                ui.ShowMessage(LocalizationManager.Localize("UseEnglishChars"));
            }
        }

        public void OnSendCodeBtnClick()
        {
            ui.Restore(restore_nick.text, restore_newPassword.text);
        }

        public void OnApplyBtnClick()
        {
            ui.ConfirmRestore(restore_code.text);
        }

        public void OnRestore(OperationMessage success)
        {
            HelperUI.ColorInputField(restore_nick, success.Type == OperationMessage.OperationType.Success ? defaultColor : invalidColor);

            if (success.Type == OperationMessage.OperationType.Fail)
            {
                restore_resultPan.SetActive(true);
                restore_resultText.text = success.Message;   
            }
        }

        public void OnConfirmRestore(bool success)
        {
            ui.ShowMessage(success
                ? LocalizationManager.Localize("RestoreOk")
                : LocalizationManager.Localize("RestoreNotOk"));

            if (success)
            {
                window.SetActive(true);
                restore_window.SetActive(false);
            }
        }
        
        public void FitWindow(LayoutGroup g)
        {
            g.CalculateLayoutInputHorizontal();
            g.CalculateLayoutInputVertical();
            g.SetLayoutHorizontal();
            g.SetLayoutVertical();
        }
        public void OnEyeBtnClick()
        {
            if (passwordField.inputType == InputField.InputType.Password)
            {
                passwordField.inputType = InputField.InputType.Standard;
                eyeImage.sprite = hideSprite;
            }
            else
            {
                passwordField.inputType = InputField.InputType.Password;
                eyeImage.sprite = showSprite;
            }
            passwordField.ForceLabelUpdate();
        }
        
    }
}