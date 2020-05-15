using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace BeatSlayerServer.Multiplayer.Accounts
{
    public class Account
    {
        public int Id { get; set; }
        public string Nick { get; set; }

        /// <summary>
        /// Hashed password
        /// </summary>
        public string Password { get; set; }
        public string Email { get; set; }
        public AccountRole Role { get; set; }


        public TimeSpan InGameTime { get; set; }

        public DateTime SignUpTime { get; set; }
        public DateTime LastLoginTime { get; set; }
        public string Country { get; set; }



        public long AllScore { get; set; }
        public long RP { get; set; }
        public int PlaceInRanking { get; set; }

        //public virtual ICollection<ReplayInfo> Replays { get; set; } = new List<ReplayInfo>();


        /// <summary>
        /// Accuracy from 0 to 1 (Hits / AllCubes)
        /// </summary>
        public float Accuracy { get { return (Hits + Misses) > 0 ? Hits / (Hits + Misses) : -1; } }
        public int MaxCombo { get; set; }
        public int Hits { get; set; }
        public int Misses { get; set; }


        // Map creator stuff
        public int MapsPublished { get; set; }
        public int PublishedMapsPlayed { get; set; }
        public int PublishedMapsLiked { get; set; }





        private Account() { }
    }

    public enum AccountRole
    {
        Player,
        Moderator,
        Developer
    }
}
