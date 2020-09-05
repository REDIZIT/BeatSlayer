using System.Collections.Generic;
using BeatSlayerServer.Multiplayer.Accounts;
using InGame.Mods;
using InGame.Multiplayer;

namespace GameNet
{
    // This is container for InGame vars
    public static class Payload
    {
        /// <summary>Used for database working</summary>
        public static AccountData Account { get; set; }

        /// <summary>Used for multiplayer working</summary>
        public static ConnectedPlayer Player { get; set; }
        
        // Previous time sent to server
        public static float PrevInGameTimeUpdate { get; set; }
        public static List<int> HitSoundIds { get; set; } = new List<int>();


        public static List<Mod> GameMods = new List<Mod>()
        {
            new SlowDownMod()
            {
                isActive = true
            }
        };
    }
}