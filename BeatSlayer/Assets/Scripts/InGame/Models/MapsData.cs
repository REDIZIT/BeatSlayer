using System;
using System.Collections.Generic;

namespace InGame.Models
{
    /// <summary>
    /// Data model of maps for trackname.<br/>
    /// <b>Server returns <see cref="MapsData"/> on game request all maps list</b>
    /// </summary>
    public class MapsData
    {
        public string Name { get; set; }
        public string Author { get; set; }
        

        public int Downloads { get; set; }
        public int PlayCount { get; set; }
        public int Likes { get; set; }
        public int Dislikes { get; set; }


        public DateTime UpdateTime { get; set; }
        public List<string> MappersNicks { get; set; }
    }
}
