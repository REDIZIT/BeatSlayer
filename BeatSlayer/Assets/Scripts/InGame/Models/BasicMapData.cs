using Newtonsoft.Json;
using ProjectManagement;
using System;
using System.Collections.Generic;
using System.Linq;

namespace InGame.Models
{
    public class BasicMapData
    {
        public string Author { get; set; }
        public string Name { get; set; }
        public string Nick { get; set; }
        public bool IsApproved { get; set; }
        public GroupType MapType { get; set; } = GroupType.Author;

        [JsonIgnore] public string Trackname { get { return Author + "-" + Name; } }

       
        public BasicMapData() { }
        public BasicMapData(ProjectMapInfo map)
        {
            Author = map.Author;
            Name = map.Name;
            Nick = map.nick;
            IsApproved = map.approved;
        }
    }

    public class OwnMapData : BasicMapData
    {
        public string Filepath { get; set; }

        public OwnMapData()
        {
            MapType = GroupType.Own;
        }
    }

    public class FullMapData : BasicMapData
    {
        [JsonIgnore] public int Downloads { get { return downloads; } }
        [JsonIgnore] public int PlayCount { get { return Difficulties.Sum(c => c.playCount); } }
        [JsonIgnore] public int Likes { get { return Difficulties.Sum(c => c.likes); } }
        [JsonIgnore] public int Dislikes { get { return Difficulties.Sum(c => c.dislikes); } }

        public int downloads;

        public List<DifficultyInfo> Difficulties { get; set; }

        public DateTime PublishTime { get; set; }
        //public DateTime ApprovedTime { get; set; }
    }
}
