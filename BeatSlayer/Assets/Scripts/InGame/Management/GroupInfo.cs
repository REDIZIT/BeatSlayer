namespace ProjectManagement
{
    public class GroupInfo
    {
        public string author, name;
        public int mapsCount;
        
        public GroupType groupType;
        public enum GroupType
        {
            Author, Own, Tutorial
        }
    }
}