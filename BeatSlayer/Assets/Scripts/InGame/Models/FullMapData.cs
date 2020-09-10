using Newtonsoft.Json;
using ProjectManagement;
using System;
using System.Collections.Generic;
using System.Linq;

namespace InGame.Models
{
    public class FullMapData : BasicMapData
    {
        [JsonIgnore] public int Downloads { get { return downloads; } }
        [JsonIgnore] public int PlayCount { get { return Difficulties.Sum(c => c.playCount); } }
        [JsonIgnore] public int Likes { get { return Difficulties.Sum(c => c.likes); } }
        [JsonIgnore] public int Dislikes { get { return Difficulties.Sum(c => c.dislikes); } }

        public int downloads;

        public List<DifficultyInfo> Difficulties { get; set; } = new List<DifficultyInfo>();

        public DateTime PublishTime { get; set; }

        public FullMapData() { }
        public FullMapData(MapsData mapsDataParent)
        {
            Author = mapsDataParent.Author;
            Name = mapsDataParent.Name;
            MapType = mapsDataParent.MapType;
        }
    }
}
