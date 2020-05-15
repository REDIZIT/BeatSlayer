using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using Microsoft.AspNetCore.SignalR.Client;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UI;
using BeatSlayerServer.Multiplayer.Accounts;
using Profile;

namespace Multiplayer.Accounts
{
    public class AccountUI : MonoBehaviour
    {
        MultiplayerCore core;
        public AccountSignUI signUI;
        public ProfileUI profileUI;
        public ProfileEditUI profileEditUI;
        
        private bool isLoginBySession = false;
        private string sessionToWrite = "";
        private float inGameTimeSent;

        public GameObject messageWindow;
        public Text messageBodyText;
        

        public void OnConnect(MultiplayerCore core)
        {
            this.core = core;
            LogInBySession();
        }

        private void Update()
        {
            if (core == null || core.account == null) return;
            core.account.InGameTime += TimeSpan.FromSeconds(Time.unscaledDeltaTime);
            if(Time.realtimeSinceStartup - inGameTimeSent > 30) UpdateInGameTime();
        }


        public void OnProfileBtnClick()
        {
            if (core.account == null)
            {
                signUI.ShowLogIn();
            }
            else
            {
                profileUI.ShowOwnAccount();
            }
        }
        public void ShowMessage(string msg)
        {
            messageWindow.SetActive(true);
            messageBodyText.text = msg;
        }
        
        
        
        
        void LogInBySession()
        {
            string path = Application.persistentDataPath + "/data/account/.session";
            if (!File.Exists(path)) return;

            string encrypted = File.ReadAllText(path);
            string decrypted = "|";
            try
            {
                decrypted = StringCipher.Decrypt(encrypted, "Wtf? Go away");
            }
            catch
            {
                File.Delete(path);
                return;
            }
            Debug.Log("Decrypted: " + decrypted);
            string[] lines = decrypted.Split('|');
            string nick = lines[0];
            string password = lines[1];
            
            Debug.Log("Auto log in with " + nick + " and " + password);
            isLoginBySession = true;
            LogIn(nick, password);
        }

        public void RefreshSession(string password)
        {
            sessionToWrite = core.account.Nick + "|" + password;
            DeleteSession();
            CreateSession();
        }
        void CreateSession()
        {
            if (isLoginBySession) return;
            
            string path = Application.persistentDataPath + "/data/account/.session";
            
            string content = StringCipher.Encrypt(sessionToWrite, "Wtf? Go away");
            sessionToWrite = "";
            
            File.WriteAllText(path, content);
        }
        void DeleteSession()
        {
            File.Delete(Application.persistentDataPath + "/data/account/.session");
            isLoginBySession = false;
            sessionToWrite = "";
        }

        public void LogIn(string nick, string password)
        {
            core.conn.InvokeAsync("Accounts_LogIn", nick, password);
            sessionToWrite = nick + '|' + password;
        }
        public void SignUp(string nick, string password, string country, string email)
        {
            core.conn.InvokeAsync("Accounts_SignUp", nick, password, country, email);
        }
        public void LogOut()
        {
            DeleteSession();
            core.account = null;
            
        }

        public void UpdateInGameTime()
        {
            if (core.account == null) return;
            float toSend = Time.realtimeSinceStartup - inGameTimeSent;
            inGameTimeSent = Time.realtimeSinceStartup;

            core.conn.InvokeAsync("Accounts_UpdateInGameTime", core.account.Nick, Mathf.Round(toSend));
        }
        
        public void Restore(string nick, string password)
        {
            core.conn.InvokeAsync("Accounts_Restore", nick, password);
        }
        public void ConfirmRestore(string code)
        {
            core.conn.InvokeAsync("Accounts_ConfirmRestore", code);
        }

        public void OnChangePassword(OperationMessage msg)
        {
            profileEditUI.OnChangePassword(msg);
        }

        public void OnChangeEmail(OperationMessage msg)
        {
            profileEditUI.OnChangeEmail(msg);
        }
        
        
        public void ViewAccount(string nick)
        {
            core.conn.InvokeAsync("Accounts_View", nick);
        }




        public void SaveAvatarToCache(bool force = false)
        {
            if (core.account == null) return;
            
            string filepath = Application.persistentDataPath + "/data/account/avatar.pic";
            /*if (!force && File.Exists(filepath))
            {
                byte[] bytes = File.ReadAllBytes(filepath);
                profileUI.OnGetAvatar(bytes);
                return;
            }
            */
            Web.WebAPI.GetAvatar(core.account.Nick, bytes =>
            {
                File.WriteAllBytes(filepath, bytes);
                profileUI.OnGetAvatar(bytes);
            });
        }

        public void SaveBackgroundToCache(bool force = false)
        {
            if (core.account == null) return;
            
            string filepath = Application.persistentDataPath + "/data/account/background.pic";
            /*if (!force && File.Exists(filepath))
            {
                byte[] bytes = File.ReadAllBytes(filepath);
                profileUI.OnGetBackground(bytes);
                return;
            }
            */
            Web.WebAPI.GetBackground(core.account.Nick, bytes =>
            {
                File.WriteAllBytes(filepath, bytes);
                profileUI.OnGetBackground(bytes);
            });
        }
        
        
        
        
        public void OnLogIn(OperationMessage op)
        {
            Debug.Log("OnLogIn " + op.Message);
            if (!isLoginBySession)
            {
                signUI.OnLogInResult(op);
            }
            if (op.Type == OperationMessage.OperationType.Success)
            {
                core.account = JsonConvert.DeserializeObject<Account>(op.Message);
                CreateSession();
                SaveAvatarToCache();
                SaveBackgroundToCache();
            }
        }

        public void OnSignUp(OperationMessage op)
        {
            signUI.OnSignUpResult(op);
        }
        public void OnAccountView(string json)
        {
            OperationResult result = JsonConvert.DeserializeObject<OperationResult>(json);
            Account acc = JsonConvert.DeserializeObject<Account>(json);
        }

        public void OnRestore(OperationMessage success)
        {
            signUI.OnRestore(success);
        }

        public void OnConfirmRestore(bool success)
        {
            signUI.OnConfirmRestore(success);
        }
    }
}
