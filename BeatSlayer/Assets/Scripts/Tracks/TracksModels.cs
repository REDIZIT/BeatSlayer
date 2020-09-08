using System.Collections.Generic;

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

    public TrackGroupClass() { }
}