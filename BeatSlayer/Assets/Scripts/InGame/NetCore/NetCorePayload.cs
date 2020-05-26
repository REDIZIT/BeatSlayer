using System;
using System.Collections;
using System.Collections.Generic;
using BeatSlayerServer.Multiplayer.Accounts;

namespace GameNet
{
    // This is container for InGame vars
    public static class NetCorePayload
    {
        public static AccountData CurrentAccount { get; set; }
        
        // Previous time sent to server
        public static float PrevInGameTimeUpdate { get; set; }
    }
}