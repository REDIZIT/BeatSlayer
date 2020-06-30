using System;
using System.Collections.Generic;
using Notifications;

namespace BeatSlayerServer.Multiplayer.Accounts
{
    public enum AccountRole
    {
        Player,
        Moderator,
        Developer
    }
    
    
    public class AccountData
    {
        
       

        public int Id { get; set; }
        public string Nick { get; set; }

        /// <summary>
        /// Hashed password
        /// </summary>
        public string Password { get; set; }
        public string Email { get; set; }
        public AccountRole Role { get; set; }



        public List<AccountData> Friends { get; set; } = new List<AccountData>();
        public List<NotificationInfo> Notifications { get; set; } = new List<NotificationInfo>();



        /*public TimeSpan InGameTime { get; set; }

        public DateTime SignUpTime { get; set; }
        public DateTime LastLoginTime { get; set; }*/
        public long InGameTimeTicks { get; set; }
        public long SignUpTimeUtcTicks { get; set; }
        public long LastLoginTimeUtcTicks { get; set; }
        public long LastActiveTimeUtcTicks { get; set; }

        public TimeSpan InGameTime => new TimeSpan(InGameTimeTicks);
        public DateTime SignUpTimeUtc => new DateTime(SignUpTimeUtcTicks);
        public DateTime LastLoginTimeUtc => new DateTime(LastLoginTimeUtcTicks);
        public DateTime LastActiveTimeUtc => new DateTime(LastActiveTimeUtcTicks);


        public bool IsOnline => (DateTime.UtcNow - LastActiveTimeUtc).TotalMinutes < 3;





        public string Country { get; set; }



        public long AllScore { get; set; }
        public long RP { get; set; }
        public int PlaceInRanking { get; set; }

        //public virtual ICollection<ReplayInfo> Replays { get; set; } = new List<ReplayInfo>();


        /// <summary>
        /// Accuracy from 0 to 1 (Hits / AllCubes)
        /// </summary>
        public float Accuracy { get { return (Hits + Misses) > 0 ? (float)Hits / (float)(Hits + Misses) : -1; } }
        public int MaxCombo { get; set; }
        public int Hits { get; set; }
        public int Misses { get; set; }
        // Count of replays grades
        public int SS { get; set; }
        public int S { get; set; }
        public int A { get; set; }
        public int B { get; set; }
        public int C { get; set; }
        public int D { get; set; }


        // Map creator stuff
        public int MapsPublished { get; set; }
        public int PublishedMapsPlayed { get; set; }
        public int PublishedMapsLiked { get; set; }


        // Shop stuff
        public int Coins { get; set; }
    }
}