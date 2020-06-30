using Newtonsoft.Json;
using System;
using UnityEngine;

namespace Ranking
{
    public class Replay
    {
        public string player;

        // MapInfo
        public string author, name, nick;
        public int difficulty;
        public string diffucltyName;

        public double RP;

        public float score;
        public int sliced, missed;

        [JsonIgnore]
        public float AllCubes
        {
            get { return sliced + missed; }
        }

        /// <summary>
        /// Accuracy in 1.0 (sliced / allCubes)
        /// </summary>
        [JsonIgnore]
        public float Accuracy
        {
            get { return AllCubes == 0 ? 0 : (float)sliced / (float)AllCubes; }
        }

        public float cubesSpeed = 1, musicSpeed = 1;

        public Replay(string author, string name, string nick, int difficulty, float score, int sliced, int missed,
            float cubesSpeed, float musicSpeed)
        {
            this.author = author;
            this.name = name;
            this.nick = nick;
            this.difficulty = difficulty;
            this.score = score;
            this.sliced = sliced;
            this.missed = missed;
            this.cubesSpeed = Mathf.Clamp(cubesSpeed, 0.5f, 1.5f);
            this.musicSpeed = Mathf.Clamp(musicSpeed, 0.5f, 1.5f);
        }

        public Replay(AccountTrackRecord record)
        {
            author = record.author;
            name = record.name;
            nick = record.nick;
            difficulty = 4; // USED DEFAULT VALUE
            score = record.score;
            sliced = record.sliced;
            missed = record.missed;
        }

        public Replay()
        {
        }
    }

    // This class is response from server on get replay
    // Used when player finished map
    public class ReplaySendData
    {
        public Grade Grade { get; set; }
        public float RP { get; set; }
        public int Coins { get; set; }
    }

    /// <summary>
    /// Replay grade (SS,S,A,B,C,D)
    /// </summary>
    public enum Grade
    {
        SS, S, A, B, C, D, Unknown
    }
}