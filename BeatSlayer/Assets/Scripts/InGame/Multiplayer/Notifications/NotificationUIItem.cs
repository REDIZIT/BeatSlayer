using System;
using System.Collections;
using System.Collections.Generic;
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


        private void Awake()
        {
            rect = GetComponent<RectTransform>();
        }

        public void Refresh(NotificationInfo notification, bool deleteAfterClick = false)
        {
            this.notification = notification;
            this.deleteAfterClick = deleteAfterClick;

            string header;
            string body;

            if (notification.Type == Notifications.NotificationType.FriendInvite)
            {
                header = "Заявка в друзья";
                body = $"<color=#f40>{notification.RequesterNick}</color> хочет добавить тебя в друзья";
            }
            else
            {
                header = "Карта рассмотрена";
                body = "[ No body ]";
            }
                    
            headerText.text = header;
            bodyText.text = body;


            bool btn_add = false, btn_decline = false, btn_later = false, btn_ok = false;

            if (notification.Type == NotificationType.FriendInvite)
            {
                btn_add = true;
                btn_decline = true;
                btn_later = true;
            }
            else if (notification.Type == NotificationType.MapModeration)
            {
                btn_ok = true;
            }
            
            addBtn.SetActive(btn_add);
            declineBtn.SetActive(btn_decline);
            laterBtn.SetActive(!deleteAfterClick && btn_later);
            okBtn.SetActive(btn_ok);

            //FitWindowSize();
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