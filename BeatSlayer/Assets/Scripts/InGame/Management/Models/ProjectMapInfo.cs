using InGame.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ProjectManagement
{
    public class ProjectMapInfo
    {
        public GroupInfo group;

        public string Author { get { return group.author; } }
        public string Name { get { return group.name; } }

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

        // If map isn't on server
        [JsonIgnore] public string filepath = "";

        public ProjectMapInfo() { }

        public ProjectMapInfo(MapsData groupData)
        {
            nick = "[LOCAL STORAGE]";
            difficultyName = "Standard";
            difficultyStars = 4;
            difficulties = new List<DifficultyInfo>()
            {
                new DifficultyInfo()
                {
                    name = "Standard",
                    stars = 4
                }
            };

            if (groupData is OwnMapsData)
            {
                filepath = (groupData as OwnMapsData).Filepath;
            }
        }
    }
}