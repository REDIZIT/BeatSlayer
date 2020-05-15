using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Multiplayer.Notification
{
    public class NotificationUIItem : MonoBehaviour
    {
        public Notification notification;

        public RectTransform rect;
        public HorizontalLayoutGroup buttonsLayoutGroup;
        
        public Text headerText, bodyText;
        public GameObject addBtn, declineBtn, laterBtn, okBtn;


        private void Awake()
        {
            rect = GetComponent<RectTransform>();
        }

        public void Refresh(Notification notification)
        {
            this.notification = notification;

            headerText.text = notification.Header;
            bodyText.text = notification.Body;


            bool btn_add = false, btn_decline = false, btn_later = false, btn_ok = false;

            if (notification.Type == NotificationType.FriendInvite)
            {
                btn_add = true;
                btn_decline = true;
                btn_later = true;
            }
            else if (notification.Type == NotificationType.Moderation)
            {
                btn_ok = true;
            }
            
            addBtn.SetActive(btn_add);
            declineBtn.SetActive(btn_decline);
            laterBtn.SetActive(btn_later);
            okBtn.SetActive(btn_ok);

            FitWindowSize();
        }

        public void FitWindowSize()
        {
            buttonsLayoutGroup.CalculateLayoutInputHorizontal();
            
            
            
            float widthByButtons = buttonsLayoutGroup.preferredWidth;
            float widthByHeader = headerText.preferredWidth;

            float width = Mathf.Max(widthByButtons, widthByHeader);
            rect.sizeDelta = new Vector2(width, 0);
            
            
            
            //bodyText.CalculateLayoutInputHorizontal();
            //bodyText.CalculateLayoutInputVertical();
            
            float heightByBody = bodyText.preferredHeight + 20;
            float height = heightByBody;

            float heightToAdd = 76 + 100;
            rect.sizeDelta = new Vector2(width, heightToAdd + height);
        }
    }
}