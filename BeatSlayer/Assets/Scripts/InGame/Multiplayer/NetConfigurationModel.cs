using GameNet;

namespace InGame.Multiplayer
{
    public class NetConfigurationModel
    {
        public NetCore.ConnectionType ServerType { get; set; } = NetCore.ConnectionType.Production;
        public string ServerUrl { get; set; } = "https://bsserver.tk";
    }
}
