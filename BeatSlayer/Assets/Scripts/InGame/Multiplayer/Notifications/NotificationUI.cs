using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Multiplayer.Notification
{
    public class NotificationUI : MonoBehaviour
    {
        public NotificationUIItem item;

        private void Start()
        {
            NotificationFriendInvite not = new NotificationFriendInvite("Shrek");
            ShowNotification(not);
        }

        public void ShowNotification(Notification notification)
        {
            item.Refresh(notification);
        }
    }

    public interface Notification
    {
         NotificationType Type { get; }
         string Header { get; }
         string Body { get; }
    }

    public class NotificationFriendInvite : Notification
    {
        public NotificationType Type
        {
            get { return NotificationType.FriendInvite; }
        }

        public string Header
        {
            get { return "Запрос в друзья"; }
        }
        public string Body
        {
            get { return $"<color=#f40>{nick}</color> хочет добавить вас в друзья"; }
        }
        
        public string nick;
        public NotificationFriendInvite(string nick)
        {
            this.nick = nick;
        }
    }

    public class NotificationModeration : Notification
    {
        public NotificationType Type { get { return NotificationType.Moderation; } }

        public string moderatorNick;
        public string map;
        public ModerationResult result;

        public NotificationModeration(string moderatorNick, string map, ModerationResult result)
        {
            this.moderatorNick = moderatorNick;
            this.map = map;
            this.result = result;
        }

        public string Header
        {
            get { return "Карта рассмотрена"; }
        }
        public string Body
        {
            get
            {
                return $"Модератор {moderatorNick} " + (result == ModerationResult.Approved ? "одобрил" : "отклонил") + " твою карту";
            }
        }
    }

    public enum NotificationType
    {
        FriendInvite,
        Moderation
    }

    public enum ModerationResult
    {
        Approved,
        Rejected
    }
}