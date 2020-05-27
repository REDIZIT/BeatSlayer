﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Multiplayer.Notification
{
    public class NotificationUI : MonoBehaviour
    {
        public NotificationUIItem item;

<<<<<<< HEAD
<<<<<<< HEAD
<<<<<<< HEAD
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
=======
        private void Start()
        {
            NotificationFriendInvite not = new NotificationFriendInvite("Shrek");
            ShowNotification(not);
>>>>>>> parent of ae2d14c... Before redesign
=======
        private void Start()
        {
=======
        private void Start()
        {
>>>>>>> parent of ae2d14c... Before redesign
            NotificationFriendInvite not = new NotificationFriendInvite("Shrek");
            ShowNotification(not);
        }

        public void ShowNotification(Notification notification)
        {
            item.Refresh(notification);
>>>>>>> parent of ae2d14c... Before redesign
        }
    }

<<<<<<< HEAD
<<<<<<< HEAD
        public void ShowNotification(Notification notification)
        {
            item.Refresh(notification);
=======
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
>>>>>>> parent of ae2d14c... Before redesign
=======
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
>>>>>>> parent of ae2d14c... Before redesign
        }
    }

<<<<<<< HEAD
<<<<<<< HEAD
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

=======
>>>>>>> parent of ae2d14c... Before redesign
=======
>>>>>>> parent of ae2d14c... Before redesign
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
<<<<<<< HEAD

    public class NotificationModeration : Notification
    {
        public NotificationType Type { get { return NotificationType.Moderation; } }

=======

    public class NotificationModeration : Notification
    {
        public NotificationType Type { get { return NotificationType.Moderation; } }

>>>>>>> parent of ae2d14c... Before redesign
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