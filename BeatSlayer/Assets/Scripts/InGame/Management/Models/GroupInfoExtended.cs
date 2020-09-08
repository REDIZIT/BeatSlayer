using System;
using System.Collections.Generic;

namespace ProjectManagement
{
    public class GroupInfoExtended : GroupInfo
    {
        public int allDownloads, allPlays, allLikes, allDislikes;

        public DateTime updateTime;
        public bool IsNew
        {
            get
            {
                return (DateTime.Now - updateTime).TotalDays <= 3;
            }
        }

        public List<string> nicks;
        public string filepath; // Path to file on phone. Used only for Own music
    }
}