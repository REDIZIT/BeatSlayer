using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Notifications
{
    public class NotificationInfo
    {
        public int Id { get; set; }
        public NotificationType Type { get; set; }

        /// <summary>
        /// Who should get this notification
        /// </summary>
        public string TargetNick { get; set; }

        /// <summary>
        /// Player who invoked notification for target
        /// </summary>
        public string RequesterNick { get; set; }
    }

    public enum NotificationType
    {
        FriendInvite,
        FriendInviteAccept,
        FriendInviteReject,
        MapModeration,
    } 
}