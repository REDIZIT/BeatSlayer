using System.Collections;
using System.Collections.Generic;
using System.IO;
using Assets.SimpleLocalization;
using InGame.Helpers;
using Microsoft.AspNetCore.SignalR.Client;
using Multiplayer.Accounts;
using UnityEngine;
using UnityEngine.UI;

namespace Profile
{
    public class ProfileEditUI : MonoBehaviour
    {
        public MultiplayerCore core;
        public AccountUI ui;
        
        public GameObject body;
        public GameObject viewBody;

        [Header("Password")]
        public InputField currentPasswordField;
        public InputField password1Field, password2Field;
        private string newpassword;

        [Header("Email")]
        public Text currentEmailText;
        public InputField newEmailField, codeField;
        private string newEmail;
        public GameObject newEmailCodeLine;
        

        public void Open()
        {
            body.SetActive(true);
            viewBody.SetActive(false);
            currentEmailText.text = (string.IsNullOrWhiteSpace(core.account.Email)) ? "-" : core.account.Email;
            newEmailCodeLine.SetActive(!string.IsNullOrWhiteSpace(core.account.Email));
        }


        public void OnAvatarBtnClick()
        {
            NativeGallery.GetImageFromGallery(new NativeGallery.MediaPickCallback(path =>
            {
                Web.WebAPI.UploadAvatar(core.account.Nick, path, message =>
                {
                    ui.SaveAvatarToCache(true);
                });
            }), LocalizationManager.Localize("SelectAvatar"));
        }


        public void OnBackgroundBtnClick()
        {
            NativeGallery.GetImageFromGallery(new NativeGallery.MediaPickCallback(path =>
            {
                Web.WebAPI.UploadBackground(core.account.Nick, path, message =>
                {
                    //ui.profileUI.OnGetBackground(File.ReadAllBytes(path));
                    ui.SaveAvatarToCache(true);
                });
            }), LocalizationManager.Localize("SelectBackground"));
        }

        public void OnPasswordChangeBtnClick()
        {
            bool canContinue = true;
            HelperUI.ColorInputField(currentPasswordField, true);
            HelperUI.ColorInputField(password1Field, true);
            HelperUI.ColorInputField(password2Field, true);

            if (currentPasswordField.text.Trim() == "")
            {
                HelperUI.ColorInputField(currentPasswordField, false);
                canContinue = false;
            }

            if (password1Field.text != password2Field.text)
            {
                HelperUI.ColorInputField(password2Field, false);
                canContinue = false;
            }

            if (canContinue)
            {
                newpassword = password1Field.text;
                core.conn.InvokeAsync("Accounts_ChangePassword", core.account.Nick, currentPasswordField.text, newpassword);
            }
        }
        public void OnEmailChangeBtnClick()
        {
            bool canContinue = true;
            bool isEmptyEmail = string.IsNullOrWhiteSpace(core.account.Email);
            
            HelperUI.ColorInputField(newEmailField, true);
            HelperUI.ColorInputField(codeField, true);
            

            if (!newEmailField.text.Contains("@"))
            {
                HelperUI.ColorInputField(newEmailField, false);
                canContinue = false;
            }

            if (!isEmptyEmail && codeField.text.Trim() == "")
            {
                HelperUI.ColorInputField(codeField, false);
                canContinue = false;
            }

            if (canContinue)
            {
                if (isEmptyEmail)
                {
                    newEmail = newEmailField.text;
                    core.conn.InvokeAsync("Accounts_ChangeEmptyEmail", core.account.Nick, newEmailField.text);
                }
                else core.conn.InvokeAsync("Accounts_ChangeEmail", core.account.Nick, codeField.text);
            }
        }
        public void OnEmailChangeCodeBtnClick()
        {
            bool canContinue = true;
            if (!newEmailField.text.Contains("@"))
            {
                HelperUI.ColorInputField(newEmailField, false);
                canContinue = false;
            }

            if (canContinue)
            {
                newEmail = newEmailField.text;
                core.conn.InvokeAsync("Accounts_SendChangeEmailCode", core.account.Nick, newEmail);   
            }
        }
        
        
        
        

        public void OnChangePassword(OperationMessage msg)
        {
            ui.ShowMessage(LocalizationManager.Localize(msg.Type == OperationMessage.OperationType.Success ?
                "PasswordChangeOk" : "PasswordChangeNotOk"));

            if (msg.Type == OperationMessage.OperationType.Success)
            {
                ui.RefreshSession(newpassword);
            }
        }

        public void OnChangeEmail(OperationMessage msg)
        {
            if (msg.Type == OperationMessage.OperationType.Success)
            {
                currentEmailText.text = newEmail;
                core.account.Email = newEmail;
                newEmailCodeLine.SetActive(true);
                ui.ShowMessage(LocalizationManager.Localize("EmailChangeOk"));
            }
            else
            {
                ui.ShowMessage(LocalizationManager.Localize(msg.Message));
            }
        }
    }

}
