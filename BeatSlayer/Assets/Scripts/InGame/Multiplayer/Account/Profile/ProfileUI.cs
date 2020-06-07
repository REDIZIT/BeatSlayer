using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Assets.SimpleLocalization;
using BeatSlayerServer.Multiplayer.Accounts;
using GameNet;
using InGame.Helpers;
using Newtonsoft.Json;
using ProjectManagement;
using UnityEngine;
using UnityEngine.UI;

namespace Profile
{
    public class ProfileUI : MonoBehaviour
    {
        public ProfileEditUI editui;
        public FriendsUI friendsUI;
        public AccountData data;

        public Animator anim;

        public GameObject body, editBody;

        [Header("Header")] 
        public RawImage avatarImage;
        public Text nickText;
        public RawImage backgroundImage;
        public Texture2D defaultBackground, defaultAvatar;
        public RectTransform header;
        public GameObject devIcon;
        public Text onlineText;
        public GameObject onlineCircle;

        [Header("Buttons")]
        public Button writeBtn;
        public Button addFriendBtn, removeFriendBtn, editBtn, endEditBtn, logoutBtn;

        [Header("Statistics")] 
        public Text placeText;
        public Text inGameText, RPText, accuracyText, scoreText, hitText, maxComboText;
        public HorizontalLayoutGroup shortStatLayout;
        public GameObject publishedMapsContent;
        public Text publishedMapsText, publishedPlayText, publishedLikesText;
        public Transform bestReplaysContent;
        public GameObject noBestResultsText;



        

        private void Update()
        {
            if (Payload.CurrentAccount == null || data == null) return;
            if (data.Nick == Payload.CurrentAccount.Nick) inGameText.text = GetTimeString(Payload.CurrentAccount.InGameTime);
        }

        public void OnEditBtnClick()
        {
            editui.Open();

            ShowButtons(false, Payload.CurrentAccount);
        }

        public void OnEndEditBtnClick()
        {
            Open();
            ShowButtons(true, Payload.CurrentAccount);
        }
        
        
        void Open()
        {
            body.SetActive(true);
            editBody.SetActive(false);
            editBtn.gameObject.SetActive(true);
            endEditBtn.gameObject.SetActive(false);
        }
        public void ShowOwnAccount()
        {
            if (Payload.CurrentAccount == null) return;
            ShowAccount(Payload.CurrentAccount);
        }

        public void ShowAccount(AccountData data)
        {
            Open();
            this.data = data;

            
            avatarImage.texture = defaultAvatar;
            backgroundImage.texture = defaultBackground;
            
            GetAvatar(data.Nick);
            GetBackground(data.Nick);
            
            FitHeaderBackgrounImage();
            
            
            
            anim.Play("Show");
            nickText.text = data.Nick;
            devIcon.SetActive(data.Nick == "REDIZIT");

            bool isOnline = data.Nick == Payload.CurrentAccount.Nick ? true : data.IsOnline;
            onlineCircle.SetActive(isOnline);
            onlineText.text = isOnline
                ? LocalizationManager.Localize("Online")
                : LocalizationManager.Localize("WasOnline", GetDateTimeString(data.LastActiveTimeUtc.ToLocalTime()));


            ShowButtons(true, data);
            

            placeText.text = data.PlaceInRanking <= 0 ? "-" : "#" + data.PlaceInRanking;
            inGameText.text = GetTimeString(data.InGameTime);
            RPText.text = data.RP + "";
            accuracyText.text = data.Accuracy == -1 ? "-" : Mathf.Floor(data.Accuracy * 10000) / 100f + "%";
            scoreText.text = data.AllScore + "";
            hitText.text = data.Hits + "";
            maxComboText.text = data.MaxCombo >= 0 ? data.MaxCombo + "" : "-";
            
            placeText.transform.parent.GetComponent<TextContainer>().UpdateThis();
            inGameText.transform.parent.GetComponent<TextContainer>().UpdateThis();
            RPText.transform.parent.GetComponent<TextContainer>().UpdateThis();
            accuracyText.transform.parent.GetComponent<TextContainer>().UpdateThis();
            scoreText.transform.parent.GetComponent<TextContainer>().UpdateThis();
            hitText.transform.parent.GetComponent<TextContainer>().UpdateThis();
            maxComboText.transform.parent.GetComponent<TextContainer>().UpdateThis();
            
            shortStatLayout.CalculateLayoutInputHorizontal();
            shortStatLayout.SetLayoutHorizontal();
            
            publishedMapsText.text = data.MapsPublished + "";
            publishedPlayText.text = data.PublishedMapsPlayed + "";
            publishedLikesText.text = data.PublishedMapsLiked + "";

            LoadBestReplays();
        }

        public void OnAddFriendBtnClick()
        {
            //NetCore.ServerActions.Friends.AddFriend(data.Nick, NetCorePayload.CurrentAccount.Nick);
            //NetCorePayload.CurrentAccount.Friends.Add(data);
            friendsUI.AddFriend(data.Nick);
            ShowButtons(true, data);
        }

        public void OnRemoveFriendBtnClick()
        {
            //NetCore.ServerActions.Friends.RemoveFriend(data.Nick, NetCorePayload.CurrentAccount.Nick);
            Payload.CurrentAccount.Friends.RemoveAll(c => c.Nick == data.Nick);
            friendsUI.RemoveFriend(data.Nick);
            ShowButtons(true, data);
        }
        
        
        
        


        void LoadBestReplays()
        {
            GameObject prefab;
            NetCore.Subs.Accounts_OnGetBestReplays += list =>
            {
                noBestResultsText.SetActive(list.Count == 0);
                HelperUI.FillContent<ReplayItemUI, ReplayData>(bestReplaysContent, list, (ui, item) =>
                {
                    ui.Refresh(item);
                });
            };
            prefab = HelperUI.ClearContent(bestReplaysContent);
            NetCore.ServerActions.Account.GetBestReplays(data.Nick, 5);
        }

        void ShowButtons(bool isViewPage, AccountData data)
        {
            bool isOwnAccount = Payload.CurrentAccount != null && Payload.CurrentAccount.Nick == data.Nick;

            if (isViewPage)
            {
                bool isFriend = Payload.CurrentAccount != null
                                && Payload.CurrentAccount.Friends.Any(c => c.Nick == data.Nick);
                
                writeBtn.gameObject.SetActive(!isOwnAccount);
                addFriendBtn.gameObject.SetActive(!isOwnAccount && !isFriend);
                removeFriendBtn.gameObject.SetActive(!isOwnAccount && isFriend);
                editBtn.gameObject.SetActive(isOwnAccount);
                logoutBtn.gameObject.SetActive(isOwnAccount);
            }
            else
            {
                endEditBtn.gameObject.SetActive(true);
            
                writeBtn.gameObject.SetActive(false);
                addFriendBtn.gameObject.SetActive(false);
                editBtn.gameObject.SetActive(false);
            }
        }


        
        public void GetAvatar(string nick)
        {
            Web.WebAPI.GetAvatar(nick, bytes =>
            {
                OnGetAvatar(bytes);
            });
        }
        public void GetBackground(string nick)
        {
            Web.WebAPI.GetBackground(nick, bytes =>
            {
                OnGetBackground(bytes);
            });
        }
        public void OnGetAvatar(byte[] bytes)
        {
            avatarImage.texture = ProjectManager.LoadTexture(bytes);
        }
        public void OnGetBackground(byte[] bytes)
        {
            if (bytes == null || bytes.Length == 0)
            {
                backgroundImage.texture = defaultBackground;
            }
            else backgroundImage.texture = ProjectManager.LoadTexture(bytes);
            FitHeaderBackgrounImage();
        }
        
        
        
        
        public void FitHeaderBackgrounImage()
        {
            float headerWidth = header.rect.width;
            float headerHeight = header.rect.height;

            float bgWidth = backgroundImage.texture.width;
            float bgHeight = backgroundImage.texture.height;
            //float bgWidth = backgroundImage.sprite.rect.width;
            //float bgHeight = backgroundImage.sprite.rect.height;

            float scale = headerWidth / bgWidth;
            
            backgroundImage.rectTransform.sizeDelta = new Vector2(headerWidth, bgHeight * scale);
        }
        public string GetTimeString(TimeSpan t)
        {
            int days = t.Days;
            int hours = t.Hours;
            int minutes = t.Minutes;
            int seconds = t.Seconds;

            string d = LocalizationManager.Localize("d");
            string h = LocalizationManager.Localize("h");
            string m = LocalizationManager.Localize("m");
            string s = LocalizationManager.Localize("s");

            return $"{days}{d} {hours}{h} {minutes}{m} {seconds}{s}";
        }

        public string GetDateTimeString(DateTime dt)
        {
            TimeSpan df = DateTime.Now - dt;
            int dfDays = DateTime.Now.DayOfYear - dt.DayOfYear;
            
            if (dfDays == 0)
            {
                if (df.Hours < 1)
                {
                    return (df.Minutes < 10 ? "0" + df.Minutes : "" + df.Minutes) + LocalizationManager.Localize("m") + " " + LocalizationManager.Localize("ago");    
                }
                return df.Hours + LocalizationManager.Localize("h") + " " + 
                       (df.Minutes < 10 ? "0" + df.Minutes : "" + df.Minutes) + LocalizationManager.Localize("m") + " " +
                       LocalizationManager.Localize("ago");
            }
            else
            {
                if (dfDays == 1)
                {
                    return LocalizationManager.Localize("yesterday") + " " + dt.ToLongTimeString();
                }
                return dt.ToLongDateString() + " " + dt.ToLongTimeString();
            }
        }
    }
}