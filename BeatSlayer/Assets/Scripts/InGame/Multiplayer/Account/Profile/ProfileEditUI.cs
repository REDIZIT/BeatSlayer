using System.Collections;
using System.Collections.Generic;
using System.IO;
using Assets.SimpleLocalization;
using GameNet;
using InGame.Helpers;
using Microsoft.AspNetCore.SignalR.Client;
using Multiplayer.Accounts;
using ProjectManagement;
using UnityEngine;
using UnityEngine.UI;

namespace Profile
{
    public class ProfileEditUI : MonoBehaviour
    {
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
            currentEmailText.text = (string.IsNullOrWhiteSpace(NetCorePayload.CurrentAccount.Email)) ? "-" : NetCorePayload.CurrentAccount.Email;
            newEmailCodeLine.SetActive(!string.IsNullOrWhiteSpace(NetCorePayload.CurrentAccount.Email));
        }


        public void OnAvatarBtnClick()
        {
            //string path = Application.persistentDataPath + "/newavatar.jpg";

            NativeGallery.GetImageFromGallery(new NativeGallery.MediaPickCallback(path =>
            {
                Texture2D tex = ProjectManager.LoadTexture(path);
                ImageCropper.Instance.Show(tex, (result, image, croppedImage) =>
                {
                    if (result)
                    {
                        ui.profileUI.avatarImage.texture = croppedImage;
                        Web.WebAPI.UploadAvatar(NetCorePayload.CurrentAccount.Nick, croppedImage, message =>
                        {
                            ui.SaveAvatarToCache(true);
                        });   
                    }
                }, new ImageCropper.Settings()
                {
                    markTextureNonReadable = false, ovalSelection = false, selectionMaxAspectRatio = 1, selectionMinAspectRatio = 1
                });
            }), LocalizationManager.Localize("SelectAvatar"));
        }


        public void OnBackgroundBtnClick()
        {
            NativeGallery.GetImageFromGallery(new NativeGallery.MediaPickCallback(path =>
            {
                Texture2D tex = ProjectManager.LoadTexture(path);
                ImageCropper.Instance.Show(tex, (result, image, croppedImage) =>
                {
                    if (result)
                    {
                        ui.profileUI.backgroundImage.texture = croppedImage;
                        Web.WebAPI.UploadBackground(NetCorePayload.CurrentAccount.Nick, croppedImage, message =>
                        {
                            ui.SaveBackgroundToCache(true);
                        });   
                    }
                }, new ImageCropper.Settings()
                {
                    markTextureNonReadable = false, ovalSelection = false
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
                MultiplayerCore.conn.InvokeAsync("Accounts_ChangePassword", NetCorePayload.CurrentAccount.Nick, currentPasswordField.text, newpassword);
            }
        }
        public void OnEmailChangeBtnClick()
        {
            bool canContinue = true;
            bool isEmptyEmail = string.IsNullOrWhiteSpace(NetCorePayload.CurrentAccount.Email);
            
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
                    MultiplayerCore.conn.InvokeAsync("Accounts_ChangeEmptyEmail", NetCorePayload.CurrentAccount.Nick, newEmailField.text);
                }
                else MultiplayerCore.conn.InvokeAsync("Accounts_ChangeEmail", NetCorePayload.CurrentAccount.Nick, codeField.text);
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
                MultiplayerCore.conn.InvokeAsync("Accounts_SendChangeEmailCode", NetCorePayload.CurrentAccount.Nick, newEmail);   
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
                NetCorePayload.CurrentAccount.Email = newEmail;
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
