using System.Collections.Generic;
using UnityEngine;


// Class that will be saved into file and used when no internet access
public class TracksDatabase
{
    public List<TrackGroupClass> tracks;
}

public class TrackGroupClass
{
    public string author, name/*, time*/;
    public int mapsCount;

    // If track is custom. This is path to local file
    public string filepath = "";

    // Sum of stats for all maps in this group
    public int downloads, plays, likes, dislikes;
    public bool novelty;
}

public class TrackClass
{
    public TrackGroupClass group;
    public string nick;

    public int likes, dislikes, downloads, plays;
    public Sprite cover;

    public bool hasUpdate;

    public int difficulty;
    public string difficultyName;
}

public class TrackRecordGroup
{
    public List<TrackRecord> ls = new List<TrackRecord>();
}
public class TrackRecord
{
    public string author, name, nick;
    public string score;
    public string multiplier;
}