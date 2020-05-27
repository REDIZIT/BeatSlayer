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
        public Texture2D defaultBackground;
        //public Image backgroundImage;
        public RectTransform header;
        public GameObject devIcon;

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



        

        private void Update()
        {
            if (NetCorePayload.CurrentAccount == null) return;
            inGameText.text = GetTimeString(NetCorePayload.CurrentAccount.InGameTime);
        }

        public void OnEditBtnClick()
        {
            editui.Open();

            ShowButtons(false, NetCorePayload.CurrentAccount);
        }

        public void OnEndEditBtnClick()
        {
            Open();
            ShowButtons(true, NetCorePayload.CurrentAccount);
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
            if (NetCorePayload.CurrentAccount == null) return;
            this.data = NetCorePayload.CurrentAccount ;

            GetAvatar(NetCorePayload.CurrentAccount.Nick);
            GetBackground(NetCorePayload.CurrentAccount.Nick);
            
            Open();
            
            anim.Play("Show");
            nickText.text = NetCorePayload.CurrentAccount.Nick;
            devIcon.SetActive(NetCorePayload.CurrentAccount.Nick == "REDIZIT");
            Debug.Log("Show page for " + NetCorePayload.CurrentAccount.Nick + " with " + NetCorePayload.CurrentAccount.Accuracy + " accuracy");


            ShowButtons(true, NetCorePayload.CurrentAccount);
            

            placeText.text = NetCorePayload.CurrentAccount.PlaceInRanking <= 0 ? "-" : "#" + NetCorePayload.CurrentAccount.PlaceInRanking;
            inGameText.text = GetTimeString(NetCorePayload.CurrentAccount.InGameTime);
            RPText.text = NetCorePayload.CurrentAccount.RP + "";
            accuracyText.text = NetCorePayload.CurrentAccount.Accuracy == -1 ? "-" : Mathf.Floor(NetCorePayload.CurrentAccount.Accuracy * 10000) / 100f + "%";
            scoreText.text = NetCorePayload.CurrentAccount.AllScore + "";
            hitText.text = NetCorePayload.CurrentAccount.Hits + "";
            maxComboText.text = NetCorePayload.CurrentAccount.MaxCombo + "";
            
            placeText.transform.parent.GetComponent<TextContainer>().UpdateThis();
            inGameText.transform.parent.GetComponent<TextContainer>().UpdateThis();
            RPText.transform.parent.GetComponent<TextContainer>().UpdateThis();
            accuracyText.transform.parent.GetComponent<TextContainer>().UpdateThis();
            scoreText.transform.parent.GetComponent<TextContainer>().UpdateThis();
            hitText.transform.parent.GetComponent<TextContainer>().UpdateThis();
            maxComboText.transform.parent.GetComponent<TextContainer>().UpdateThis();
            
            shortStatLayout.CalculateLayoutInputHorizontal();
            shortStatLayout.SetLayoutHorizontal();
            
            /*publishedMapsContent.SetActive(false);
            if (core.account.MapsPublished > 0)
            {
                publishedMapsContent.SetActive(true);
                publishedMapsText.text = core.account.MapsPublished + "";
                publishedPlayText.text = core.account.PublishedMapsPlayed + "";
                publishedLikesText.text = core.account.PublishedMapsLiked + "";
            }*/
            publishedMapsText.text = NetCorePayload.CurrentAccount.MapsPublished + "";
            publishedPlayText.text = NetCorePayload.CurrentAccount.PublishedMapsPlayed + "";
            publishedLikesText.text = NetCorePayload.CurrentAccount.PublishedMapsLiked + "";

            LoadBestReplays();
        }

        public void ShowAccount(AccountData data)
        {
            Open();
            this.data = data;

            GetAvatar(data.Nick);
            GetBackground(data.Nick);
            
            
            
            anim.Play("Show");
            nickText.text = data.Nick;
            devIcon.SetActive(data.Nick == "REDIZIT");
            Debug.Log("Show page for " + data.Nick + " with " + data.Accuracy + " accuracy");


            ShowButtons(true, data);
            

            placeText.text = data.PlaceInRanking <= 0 ? "-" : "#" + data.PlaceInRanking;
            inGameText.text = GetTimeString(data.InGameTime);
            RPText.text = data.RP + "";
            accuracyText.text = data.Accuracy == -1 ? "-" : Mathf.Floor(data.Accuracy * 10000) / 100f + "%";
            scoreText.text = data.AllScore + "";
            hitText.text = data.Hits + "";
            maxComboText.text = data.MaxCombo + "";
            
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
            NetCorePayload.CurrentAccount.Friends.Add(data);
            friendsUI.AddFriend(data.Nick);
            ShowButtons(true, data);
        }

        public void OnRemoveFriendBtnClick()
        {
            //NetCore.ServerActions.Friends.RemoveFriend(data.Nick, NetCorePayload.CurrentAccount.Nick);
            NetCorePayload.CurrentAccount.Friends.RemoveAll(c => c.Nick == data.Nick);
            friendsUI.RemoveFriend(data.Nick);
            ShowButtons(true, data);
        }
        
        
        
        


        void LoadBestReplays()
        {
            GameObject prefab;
            NetCore.Subs.Accounts_OnGetBestReplays += list =>
            {
                HelperUI.FillContent<ReplayItemUI, ReplayData>(bestReplaysContent, list, (ui, item) =>
                {
                    ui.Refresh(item);
                });
            };
            prefab = HelperUI.ClearContent(bestReplaysContent);
            NetCore.ServerActions.Account.GetBestReplays(NetCorePayload.CurrentAccount.Nick, 5);
        }

        void ShowButtons(bool isViewPage, AccountData data)
        {
            bool isOwnAccount = NetCorePayload.CurrentAccount != null && NetCorePayload.CurrentAccount.Nick == data.Nick;

            if (isViewPage)
            {
                bool isFriend = NetCorePayload.CurrentAccount != null
                                && NetCorePayload.CurrentAccount.Friends.Any(c => c.Nick == data.Nick);
                
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
            /*bool loadFromWeb = false;
            if (NetCorePayload.CurrentAccount == null)
            {
                loadFromWeb = true;
            }
            else
            {
                loadFromWeb = NetCorePayload.CurrentAccount.Nick != nick;
            }

            if (loadFromWeb)
            {
                
            }
            else OnGetAvatar(File.ReadAllBytes(Application.persistentDataPath + "/data/account/avatar.pic"));*/
        }
        public void GetBackground(string nick)
        {
            Web.WebAPI.GetBackground(nick, bytes =>
            {
                OnGetBackground(bytes);
            });
            /*bool loadFromWeb = false;
            if (NetCorePayload.CurrentAccount == null)
            {
                loadFromWeb = true;
            }
            else
            {
                loadFromWeb = NetCorePayload.CurrentAccount.Nick != nick;
            }

            if (loadFromWeb)
            {
                Web.WebAPI.GetBackground(nick, bytes =>
                {
                    OnGetBackground(bytes);
                });
            }
            else OnGetBackground(File.ReadAllBytes(Application.persistentDataPath + "/data/account/background.pic"));*/
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
    }
}