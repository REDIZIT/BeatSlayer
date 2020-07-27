using GameNet;

namespace InGame.Multiplayer
{
    public class NetConfigurationModel
    {
        public NetCore.ConnectionType ServerType { get; set; } = NetCore.ConnectionType.Production;

        public string ServerUrl
        {
            get
            {
                switch(ServerType)
                {
                    case NetCore.ConnectionType.Production: return ProductionServerUrl;
                    case NetCore.ConnectionType.Development: return DevelopmentServerUrl;
                    case NetCore.ConnectionType.Local: return LocalServerUrl;
                    default: return "";
                }
            }
        }

        public string ProductionServerUrl { get; set; } = "https://bsserver.tk";
        public string DevelopmentServerUrl { get; set; } = "http://bsserver.tk:5020";
        public string LocalServerUrl { get; set; } = "https://localhost:5011";
    }
}
