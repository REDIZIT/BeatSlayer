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

        public GameObject noNotificationsText;

        public Image iconImage;
        public Color defaultIconColor, activeIconColor;
        public GameObject countGo;
        public Text countText;


        public void Awake()
        {
            /*NetCore.Configurators += () =>
            {
                
            };*/
            /*ShowNotification(new NotificationInfo()
            {
                RequesterNick = "REDIZIT",
                TargetNick = "Tester!",
                Type = NotificationType.FriendInviteAccept
            });*/
        }
        public void Configuration()
        {
            NetCore.OnLogIn += () =>
            {
                RefreshIcon();
            };
        }
        
        
        
        public void ShowNotification(NotificationInfo notification)
        {
            anim.Play("Show");
            item.Refresh(notification);
        }

        public void OnShowAllBtnClick()
        {
            if (Payload.Account == null) return;

            window.SetActive(true);
            noNotificationsText.SetActive(Payload.Account.Notifications.Count == 0);

            HelperUI.FillContent<NotificationUIItem, NotificationInfo>(content,
                Payload.Account.Notifications,
                (uiItem, info) =>
                {
                    uiItem.Refresh(info, true);
                });
        }



        public void RefreshIcon()
        {
            if (Payload.Account == null) return;
            bool active = Payload.Account.Notifications.Count > 0;
            
            countGo.SetActive(active);
            iconImage.color = active ? activeIconColor : defaultIconColor;
            countText.text = Payload.Account.Notifications.Count + "";
        }
        

        public void Hide()
        {
            anim.Play("Hide");
        }

        public void Accept(NotificationInfo not)
        {
            NetCore.ServerActions.Notifications.Accept(Payload.Account.Nick, not.Id);

            Payload.Account.Notifications.Remove(not);
            RefreshIcon();
            noNotificationsText.SetActive(Payload.Account.Notifications.Count == 0);
            Hide();
        }

        public void Reject(NotificationInfo not)
        {
            NetCore.ServerActions.Notifications.Reject(Payload.Account.Nick, not.Id);

            Payload.Account.Notifications.Remove(not);
            RefreshIcon();
            noNotificationsText.SetActive(Payload.Account.Notifications.Count == 0);
            Hide();
        }

        public void Ok(NotificationInfo not, bool deleteAfterClick)
        {
            NetCore.ServerActions.Notifications.Ok(Payload.Account.Nick, not.Id);

            Payload.Account.Notifications.Remove(not);
            RefreshIcon();
            noNotificationsText.SetActive(Payload.Account.Notifications.Count == 0);

            /// If popup
            if(!deleteAfterClick) Hide();
        }
    }
}