using System;
using System.Collections;
using System.Collections.Generic;
using BeatSlayerServer.Multiplayer.Accounts;
using InGame.Mods;

namespace GameNet
{
    // This is container for InGame vars
    public static class Payload
    {
        public static AccountData CurrentAccount { get; set; }
        
        // Previous time sent to server
        public static float PrevInGameTimeUpdate { get; set; }


        public static List<Mod> GameMods = new List<Mod>()
        {
            new SlowDownMod()
            {
                isActive = true
            }
        };
    }
}