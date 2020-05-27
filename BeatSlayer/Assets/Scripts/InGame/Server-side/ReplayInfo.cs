﻿namespace BeatSlayerServer.Multiplayer.Accounts
{
    /*public class ReplayInfo
    {
        public int Id { get; set; }
        public virtual MapInfo Map { get; set; }
        public virtual Account Player { get; set; }



        public float Score { get; set; }
        public float RP { get; set; }

        public int Missed { get; set; }
        public int CubesSliced { get; set; }
        public float Accuracy => (CubesSliced + Missed) == 0 ? 0 : Missed / (CubesSliced + Missed);
    }*/
    public class ReplayData
    {
        public MapData Map { get; set; }
        public string DifficultyName { get; set; }
        public string Nick { get; set; }

        public float Score { get; set; }
        public float RP { get; set; }

        public int Missed { get; set; }
        public int CubesSliced { get; set; }
        public float Accuracy => (CubesSliced + Missed) == 0 ? 0 : (float)CubesSliced / (float)(CubesSliced + Missed);
    }
}