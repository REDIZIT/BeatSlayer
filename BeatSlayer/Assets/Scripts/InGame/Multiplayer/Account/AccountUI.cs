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
using GameNet;
using Profile;

namespace Multiplayer.Accounts
{
    public class AccountUI : MonoBehaviour
    {
        MultiplayerMenuWrapper wrapper;
        public AccountSignUI signUI;
        public ProfileUI profileUI;
        public ProfileEditUI profileEditUI;
        
        private bool isLoginBySession = false;
        private string sessionToWrite = "";
        private float inGameTimeSent;

        public GameObject messageWindow;
        public Text messageBodyText;


        private void Awake()
        {
            NetCore.Configurators += () =>
            {
                NetCore.Subs.Accounts_OnLogIn += OnLogIn;
                NetCore.Subs.Accounts_OnSignUp += OnSignUp;
                NetCore.Subs.Accounts_OnView += OnView;
            };
        }

        public void OnConnect(MultiplayerMenuWrapper wrapper)
        {
            this.wrapper = wrapper;
            LogInBySession();
        }

        private void Update()
        {
            if (wrapper == null || NetCorePayload.CurrentAccount == null) return;
            NetCorePayload.CurrentAccount.InGameTimeTicks += TimeSpan.FromSeconds(Time.unscaledDeltaTime).Ticks;
            if(Time.realtimeSinceStartup - inGameTimeSent > 30) UpdateInGameTime();
        }


        public void OnProfileBtnClick()
        {
            if (NetCorePayload.CurrentAccount == null)
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

            string[] lines = decrypted.Split('|');
            string nick = lines[0];
            string password = lines[1];
            
            isLoginBySession = true;
            LogIn(nick, password);
        }

        public void RefreshSession(string password)
        {
            sessionToWrite = NetCorePayload.CurrentAccount.Nick + "|" + password;
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
            //MultiplayerCore.conn.InvokeAsync("Accounts_LogIn", nick, password);
            NetCore.ServerActions.Account.LogIn(nick, password);
            sessionToWrite = nick + '|' + password;
        }
        public void SignUp(string nick, string password, string country, string email)
        {
            NetCore.ServerActions.Account.SignUp(nick, password, country, email);
        }
        public void LogOut()
        {
            DeleteSession();
            NetCorePayload.CurrentAccount = null;
            
        }

        public void UpdateInGameTime()
        {
            if (NetCorePayload.CurrentAccount == null) return;
            float toSend = Time.realtimeSinceStartup - inGameTimeSent;
            inGameTimeSent = Time.realtimeSinceStartup;

            //MultiplayerCore.conn.InvokeAsync("Accounts_UpdateInGameTime", NetCorePayload.CurrentAccount.Nick, Mathf.Round(toSend));
        }
        
        public void Restore(string nick, string password)
        {
            NetCore.ServerActions.Account.Restore(nick, password);
        }
        public void ConfirmRestore(string code)
        {
            NetCore.ServerActions.Account.ConfirmRestore(code);
        }

        public void OnChangePassword(OperationMessage msg)
        {
            profileEditUI.OnChangePassword(msg);
        }

        public void OnChangeEmail(OperationMessage msg)
        {
            profileEditUI.OnChangeEmail(msg);
        }



        public void SaveAvatarToCache(bool force = false)
        {
            if (NetCorePayload.CurrentAccount== null) return;
            
            string filepath = Application.persistentDataPath + "/data/account/avatar.pic";
            /*if (!force && File.Exists(filepath))
            {
                byte[] bytes = File.ReadAllBytes(filepath);
                profileUI.OnGetAvatar(bytes);
                return;
            }
            */
            Web.WebAPI.GetAvatar(NetCorePayload.CurrentAccount.Nick, bytes =>
            {
                profileUI.OnGetAvatar(bytes);
            }, true);
        }

        public void SaveBackgroundToCache(bool force = false)
        {
            if (NetCorePayload.CurrentAccount== null) return;
            
            string filepath = Application.persistentDataPath + "/data/account/background.pic";
            /*if (!force && File.Exists(filepath))
            {
                byte[] bytes = File.ReadAllBytes(filepath);
                profileUI.OnGetBackground(bytes);
                return;
            }
            */
            Web.WebAPI.GetBackground(NetCorePayload.CurrentAccount.Nick, bytes =>
            {
                profileUI.OnGetBackground(bytes);
            }, true);
        }
        
        
        
        
        public void OnLogIn(OperationMessage op)
        {
            if (!isLoginBySession)
            {
                signUI.OnLogInResult(op);
            }
            if (op.Type == OperationMessage.OperationType.Success)
            {
                NetCorePayload.CurrentAccount = op.Account;
                CreateSession();
                SaveAvatarToCache();
                SaveBackgroundToCache();
                
<<<<<<< HEAD
<<<<<<< HEAD
<<<<<<< HEAD
                NetCore.OnLogIn();
=======
=======
>>>>>>> parent of ae2d14c... Before redesign
=======
>>>>>>> parent of ae2d14c... Before redesign
                Debug.Log("I have coins: " + op.Account.Coins);
                
                NetCore.OnLogIn?.Invoke();
>>>>>>> parent of ae2d14c... Before redesign
            }
        }

        public void OnSceneLoad()
        {
            SaveAvatarToCache();
            SaveBackgroundToCache();
            
            //wrapper.chatUI.OnLogIn();
        }

        public void OnSignUp(OperationMessage op)
        {
            signUI.OnSignUpResult(op);
        }
        public void OnView(AccountData acc)
        {
            profileUI.ShowAccount(acc);
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
