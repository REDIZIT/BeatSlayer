using System;
using System.Collections;
using System.Collections.Generic;
using Assets.SimpleLocalization;
using GameNet;
using ProjectManagement;
using UnityEngine;
using UnityEngine.UI;

namespace Profile
{
    public class ProfileUI : MonoBehaviour
    {
        public ProfileEditUI editui;

        public Animator anim;

        public GameObject body, editBody;

        [Header("Header")] 
        public RawImage avatarImage;
        public Text nickText;
        public RawImage backgroundImage;
        //public Image backgroundImage;
        public RectTransform header;
        public GameObject devIcon;

        [Header("Buttons")]
        public Button writeBtn;
        public Button addFriendBtn, editBtn, endEditBtn, logoutBtn;

        [Header("Statistics")] 
        public Text placeText;
        public Text inGameText, RPText, accuracyText, scoreText, hitText, maxComboText;
        public HorizontalLayoutGroup shortStatLayout;
        public GameObject publishedMapsContent;
        public Text publishedMapsText, publishedPlayText, publishedLikesText;



        

        private void Update()
        {
            if (NetCorePayload.CurrentAccount == null) return;
            inGameText.text = GetTimeString(NetCorePayload.CurrentAccount.InGameTime);
        }

        public void OnEditBtnClick()
        {
            editui.Open();

            ShowButtons(false);
        }

        public void OnEndEditBtnClick()
        {
            Open();
            ShowButtons(true);
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

            Open();
            
            anim.Play("Show");
            nickText.text = NetCorePayload.CurrentAccount.Nick;
            devIcon.SetActive(NetCorePayload.CurrentAccount.Nick == "REDIZIT");


            ShowButtons(true, true);
            

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
        }

        void ShowButtons(bool isViewPage, bool isOwnAccount = true)
        {
            if (isViewPage)
            {
                writeBtn.gameObject.SetActive(isOwnAccount);
                addFriendBtn.gameObject.SetActive(!isOwnAccount);
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


        public void OnGetAvatar(byte[] bytes)
        {
            avatarImage.texture = ProjectManager.LoadTexture(bytes);
        }
        public void OnGetBackground(byte[] bytes)
        {
            backgroundImage.texture = ProjectManager.LoadTexture(bytes);
            //backgroundImage.sprite = ProjectManager.LoadSprite(bytes);
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