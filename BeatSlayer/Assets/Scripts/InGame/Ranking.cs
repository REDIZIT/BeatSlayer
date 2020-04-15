using Newtonsoft.Json;
using System;

public class Replay
{
    public string player;
    // MapInfo
    public string author, name, nick;
    public int difficulty;

    public double RP;

    public float score;
    public int sliced, missed;
    [JsonIgnore] public float AllCubes { get { return sliced + missed; } }
    /// <summary>
    /// Accuracy in 1.0 (sliced / allCubes)
    /// </summary>
    [JsonIgnore] public float Accuracy { get { return AllCubes == 0 ? 0 : sliced / AllCubes; } }

    public Replay(string author, string name, string nick, int difficulty, float score, int sliced, int missed)
    {
        this.author = author;
        this.name = name;
        this.nick = nick;
        this.difficulty = difficulty;
        this.score = score;
        this.sliced = sliced;
        this.missed = missed;
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

    public Replay() { }
}