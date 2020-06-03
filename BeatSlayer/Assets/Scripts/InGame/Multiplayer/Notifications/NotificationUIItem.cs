using System;
using System.Collections;
using System.Collections.Generic;
using Assets.SimpleLocalization;
using Notifications;
using UnityEngine;
using UnityEngine.UI;

namespace Multiplayer.Notification
{
    public class NotificationUIItem : MonoBehaviour
    {
        public NotificationUI ui;
        public NotificationInfo notification;

        public RectTransform rect;
        public HorizontalLayoutGroup buttonsLayoutGroup;
        
        public Text headerText, bodyText;
        public GameObject addBtn, declineBtn, laterBtn, okBtn;

        public bool deleteAfterClick = false;

        Func<string, string> Local = LocalizationManager.Localize;


        private void Awake()
        {
            rect = GetComponent<RectTransform>();
        }

        public void Refresh(NotificationInfo notification, bool deleteAfterClick = false)
        {
            this.notification = notification;
            this.deleteAfterClick = deleteAfterClick;


            HandleType();
        }

        public void HandleType()
        {
            string header = "";
            string body = "";
            bool btn_add = false, btn_decline = false, btn_later = false, btn_ok = false;



            switch (notification.Type)
            {
                case NotificationType.FriendInvite:
                    header = Local("FriendInvite");
                    body = string.Format(Local("FriendInviteBody"), notification.RequesterNick);
                    btn_add = true;
                    btn_decline = true;
                    btn_later = true;
                    break;
                case NotificationType.FriendInviteAccept:
                    header = Local("FriendInviteAccept");
                    body = string.Format(Local("FriendInviteAcceptBody"), notification.RequesterNick);
                    btn_ok = true;
                    break;
                case NotificationType.FriendInviteReject:
                    header = Local("FriendInviteReject");
                    body = string.Format(Local("FriendInviteRejectBody"), notification.RequesterNick);
                    btn_ok = true;
                    break;
                case NotificationType.MapModeration:
                    header = "Карта рассмотрена";
                    body = "[ No body ]";
                    btn_ok = true;
                    break;
                default:
                    Debug.LogError("Notification switch error for " + notification.Type.ToString());
                    break;
            }





            headerText.text = header;
            bodyText.text = body;

            addBtn.SetActive(btn_add);
            declineBtn.SetActive(btn_decline);
            laterBtn.SetActive(!deleteAfterClick && btn_later);
            okBtn.SetActive(btn_ok);

        }




        public void OnLaterBtnClick()
        {
            ui.Hide();
            if(deleteAfterClick) Destroy(gameObject);
        }

        public void OnAcceptBtnClick()
        {
            ui.Accept(notification);
            if(deleteAfterClick) Destroy(gameObject);
        }

        public void OnRejectBtnClick()
        {
            ui.Reject(notification);
            if(deleteAfterClick) Destroy(gameObject);
        }
        
        public void OnOkBtnClick()
        {
            ui.Ok(notification);
            if (deleteAfterClick) Destroy(gameObject);
        }
        
        

        /*public void FitWindowSize()
        {
            buttonsLayoutGroup.CalculateLayoutInputHorizontal();
            
            
            
            float widthByButtons = buttonsLayoutGroup.preferredWidth;
            float widthByHeader = headerText.preferredWidth;

            float width = Mathf.Max(widthByButtons, widthByHeader);
            rect.sizeDelta = new Vector2(width, 0);
            
            
            
            bodyText.CalculateLayoutInputHorizontal();
            bodyText.CalculateLayoutInputVertical();
            
            float heightByBody = bodyText.preferredHeight + 20;
            float height = heightByBody;

            float heightToAdd = 76 + 100;
            rect.sizeDelta = new Vector2(width, heightToAdd + height);
        }*/
    }
}