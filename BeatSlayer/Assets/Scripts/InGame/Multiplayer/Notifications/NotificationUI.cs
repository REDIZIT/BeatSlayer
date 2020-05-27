using System;
using System.Collections;
using System.Collections.Generic;
using GameNet;
using InGame.Helpers;
using Notifications;
using UnityEngine;
using UnityEngine.UI;

namespace Multiplayer.Notification
{
    public class NotificationUI : MonoBehaviour
    {
        public NotificationUIItem item;
        public Animator anim;
        public Transform content;
        public GameObject window;

        public Image iconImage;
        public Color defaultIconColor, activeIconColor;
        public GameObject countGo;
        public Text countText;


        public void Awake()
        {
            NetCore.Configurators += () =>
            {
                NetCore.OnLogIn += () =>
                {
                    RefreshIcon();
                };
            };
        }
        
        
        
        public void ShowNotification(NotificationInfo notification)
        {
            anim.Play("Show");
            item.Refresh(notification);
        }

        public void OnShowAllBtnClick()
        {
            if (NetCorePayload.CurrentAccount == null) return;
            window.SetActive(true);
            HelperUI.FillContent<NotificationUIItem, NotificationInfo>(content,
                NetCorePayload.CurrentAccount.Notifications,
                (uiItem, info) =>
                {
                    uiItem.Refresh(info, true);
                });
        }



        public void RefreshIcon()
        {
            if (NetCorePayload.CurrentAccount == null) return;
            bool active = NetCorePayload.CurrentAccount.Notifications.Count > 0;
            
            countGo.SetActive(active);
            iconImage.color = active ? activeIconColor : defaultIconColor;
            countText.text = NetCorePayload.CurrentAccount.Notifications.Count + "";
        }
        
        
        
        
        

        public void Hide()
        {
            anim.Play("Hide");
        }

        public void Accept(NotificationInfo not)
        {
            NetCore.ServerActions.Notifications.Accept(NetCorePayload.CurrentAccount.Nick, not.Id);
        }

        public void Reject(NotificationInfo not)
        {
            NetCore.ServerActions.Notifications.Reject(NetCorePayload.CurrentAccount.Nick, not.Id);
        }
    }
}