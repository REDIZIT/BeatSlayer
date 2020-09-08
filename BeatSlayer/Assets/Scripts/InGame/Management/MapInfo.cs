using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ProjectManagement
{
    public class MapInfo
    {
        public GroupInfo group;
        public bool isMapDeleted;

        public string author { get { return group.author; } }
        public string name { get { return group.name; } }

        public string nick;

        /// <summary>
        /// Deprecated. Use Likes, Dislikes, PlayCount and downloads (not deprecated)
        /// </summary>
        public int likes, dislikes, playCount, downloads;

        public int Downloads { get { return downloads; } }
        public int PlayCount { get { return difficulties.Sum(c => c.playCount); } }
        public int Likes { get { return difficulties.Sum(c => c.likes); } }
        public int Dislikes { get { return difficulties.Sum(c => c.dislikes); } }

        public string difficultyName;
        public int difficultyStars;
        public List<DifficultyInfo> difficulties;
        

        public DateTime publishTime;

        public bool approved;
        public DateTime grantedTime;

        // If map isn't on server
        [JsonIgnore] public string filepath = "";

        public bool IsGrantedNow
        {
            get
            {
                if (!approved) return false;
                else return grantedTime > publishTime;
            }
        }

        public MapInfo() { }
        public MapInfo(GroupInfo group)
        {
            this.group = group;
        }
    }
}