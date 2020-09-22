using System;
using System.IO;
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
        public MenuScript_v2 menu;


        private bool isLoginBySession = false;
        private string sessionToWrite = "";
        private float inGameTimeSent;

        public GameObject messageWindow;
        public Text messageBodyText;


        public void Configuration()
        {
            NetCore.Subs.Accounts_OnLogIn += (op) =>
            {
                OnLogIn(op);
            };
            NetCore.Subs.Accounts_OnSignUp += OnSignUp;
            NetCore.Subs.Accounts_OnView += OnView;
            NetCore.Subs.Accounts_OnChangeEmail += OnChangeEmail;
            NetCore.Subs.Accounts_OnChangePassword += OnChangePassword;
            NetCore.Subs.Accounts_OnConfirmRestore += OnConfirmRestore;
        }

        public void OnConnect(MultiplayerMenuWrapper wrapper)
        {
            this.wrapper = wrapper;
            LogInBySession();
        }

        private void Update()
        {
            if (wrapper == null || Payload.Account == null) return;
            Payload.Account.InGameTimeTicks += TimeSpan.FromSeconds(Time.unscaledDeltaTime).Ticks;
            if(Time.realtimeSinceStartup - inGameTimeSent > 30) UpdateInGameTime();
        }


        public void OnProfileBtnClick()
        {
            if (Payload.Account == null)
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
            // This is old session code (for compatibility
            if(File.Exists(Application.persistentDataPath + "/session.txt"))
            {
                LogInByOldSession();
                return;
            }

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

        void LogInByOldSession()
        {
            string path = Application.persistentDataPath + "/session.txt";
            if (!File.Exists(path)) return;

            string content = File.ReadAllText(path);
            string nick = content.Split(':')[0];
            string password = content.Split(':')[1];

            File.Delete(path);
            LogIn(nick, password);
        }
        public static bool HasSession(bool old = false)
        {
            if(old) return File.Exists(Application.persistentDataPath + "/session.txt");
            else return File.Exists(Application.persistentDataPath + "/data/account/.session");
        }


        public void RefreshSession(string password)
        {
            sessionToWrite = Payload.Account.Nick + "|" + password;
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
            File.Delete(Application.persistentDataPath + "/data/account/avatar.pic");
            File.Delete(Application.persistentDataPath + "/data/account/background.pic");
            isLoginBySession = false;
            sessionToWrite = "";
        }

        public void LogIn(string nick, string password)
        {
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
            Payload.Account = null;

            NetCore.OnLogOut?.Invoke();
        }

        public void UpdateInGameTime()
        {
            if (Payload.Account == null) return;
            float toSend = Time.realtimeSinceStartup - inGameTimeSent;
            inGameTimeSent = Time.realtimeSinceStartup;
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
            if (Payload.Account== null) return;
            
            string filepath = Application.persistentDataPath + "/data/account/avatar.pic";
            /*if (!force && File.Exists(filepath))
            {
                byte[] bytes = File.ReadAllBytes(filepath);
                profileUI.OnGetAvatar(bytes);
                return;
            }
            */
            Web.WebAPI.GetAvatar(Payload.Account.Nick, bytes =>
            {
                profileUI.OnGetAvatar(bytes);
            }, true);
        }

        public void SaveBackgroundToCache(bool force = false)
        {
            if (Payload.Account== null) return;
            
            string filepath = Application.persistentDataPath + "/data/account/background.pic";
            /*if (!force && File.Exists(filepath))
            {
                byte[] bytes = File.ReadAllBytes(filepath);
                profileUI.OnGetBackground(bytes);
                return;
            }
            */
            Web.WebAPI.GetBackground(Payload.Account.Nick, bytes =>
            {
                profileUI.OnGetBackground(bytes);
            }, true);
        }
        
        
        
        
        public void OnLogIn(OperationMessage op)
        {
            /*UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                Debug.Log(" << OnLogIn dispatched");
                if (!isLoginBySession)
                {
                    Debug.Log(" << OnLogInResult");
                    signUI.OnLogInResult(op);
                }
                if (op.Type == OperationMessage.OperationType.Success)
                {
                    Payload.CurrentAccount = op.Account;
                    CreateSession();
                    SaveAvatarToCache();
                    SaveBackgroundToCache();

                    SyncCoins();

                    NetCore.OnLogIn();
                }
            });*/
            
            if (!isLoginBySession)
            {
                signUI.OnLogInResult(op);
            }
            if (op.Type == OperationMessage.OperationType.Success)
            {
                Payload.Account = op.Account;

                if(profileUI.data == null)
                {
                    SaveAvatarToCache();
                    SaveBackgroundToCache();
                }

                CreateSession();

                SyncCoins();

                NetCore.OnLogIn();
            }

            isLoginBySession = false;
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

        public void SyncCoins()
        {
            if (Payload.Account == null) return;

            if (AdvancedSaveManager.prefs == null)
            {
                Debug.LogError("SyncCoins: prefsmanager.prefs is null");
                return;
            }


            int coins = AdvancedSaveManager.prefs.coins;

            if (Payload.Account.Coins == -1)
            {
                Debug.Log("Sync coins: " + coins + " / " + Payload.Account.Coins);
                Payload.Account.Coins = coins;
                NetCore.ServerActions.Shop.SyncCoins(Payload.Account.Nick, coins);
            }
        }
    }
}
