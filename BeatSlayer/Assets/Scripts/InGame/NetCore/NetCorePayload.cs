using System.Collections;
using System.Collections.Generic;
using BeatSlayerServer.Multiplayer.Accounts;

namespace GameNet
{
    // This is container for InGame vars
    public static class NetCorePayload
    {
        public static Account CurrentAccount { get; set; }
    }
}