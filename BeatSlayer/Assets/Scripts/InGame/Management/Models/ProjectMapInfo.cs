using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ProjectManagement
{
    public class ProjectMapInfo
    {
        public GroupInfo group;
        public bool isMapDeleted;

        public string Author { get { return group.author; } }
        public string Name { get { return group.name; } }
        [JsonIgnore] public string Trackname { get { return Author + "-" + Name; } }

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

        public ProjectMapInfo() { }
        public ProjectMapInfo(GroupInfo group)
        {
            this.group = group;
        }
    }
}