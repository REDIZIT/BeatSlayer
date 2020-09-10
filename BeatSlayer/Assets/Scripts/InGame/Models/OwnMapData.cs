using ProjectManagement;

namespace InGame.Models
{
    public class OwnMapData : BasicMapData
    {
        public string Filepath { get; set; }

        public OwnMapData()
        {
            MapType = GroupType.Own;
        }
    }
}
