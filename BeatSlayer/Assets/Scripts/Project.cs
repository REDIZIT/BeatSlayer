using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Project
{
    public string author, name;
    public int mins, secs;
    public string source, creatorNick;

    public string difficultName = "Standard";
    public int difficultStars = 4;


    public bool hasImage;
    public enum ImageExtension { Jpeg, Png }
    public ImageExtension imageExtension;
    public byte[] image;

    public enum AudioExtension { Ogg, Mp3 }
    public AudioExtension audioExtension;
    public byte[] audioFile;

    public List<BeatCubeClass> beatCubeList = new List<BeatCubeClass>();

    public void CheckDefaults()
    {
        if (beatCubeList == null) beatCubeList = new List<BeatCubeClass>();
    }

    public static string ToString(ImageExtension value)
    {
        return value == ImageExtension.Jpeg ? ".jpg" : ".png";
    }
    public static string ToString(AudioExtension value)
    {
        return value == AudioExtension.Mp3 ? ".mp3" : ".ogg";
    }
}

[Serializable]
public class BeatCubeClass
{
    public float time; // Time in seconds where cube placed

    public int road; // 0,1,2,3
    public int level; // 0,1 - cube height level (0-Bottom, 1-Top)

    public enum Type { Point, Dir, Line }
    public Type type; // Type of cube: Point (Any direction), Dir (arrow), Line

    // Direction you need to cut
    public enum SubType { Down, DownRight, Right, UpRight, Up, UpLeft, Left, DownLeft, Random }
    public SubType subType;

    // For which saber (left: -1 or right: 1 or any: 0)
    public int saberType;

    // Used only for Line (position of start and end of line)
    public List<SerializableVector3> linePoints;

    // Parameterless ctor
    public BeatCubeClass() { }
    public BeatCubeClass(float time, int road, Type type)
    {
        this.time = time;
        this.road = road;
        this.type = type;
    }
    public BeatCubeClass(float time, int road, Type type, Vector3[] v3)
    {
        this.time = time;
        this.road = road;
        this.type = type;
        linePoints = new List<SerializableVector3>();
        for (int i = 0; i < v3.Length; i++)
        {
            linePoints.Add(v3[i]);
        }
    }
}

//[Serializable]
//public class Bookmark
//{
//    public float time;
//    public int type;
//    public SerializableColor color;

//    public Bookmark(float time, int type, SerializableColor color)
//    {
//        this.time = time;
//        this.type = type;
//        this.color = color;
//    }

//    public Bookmark() { }
//}

[System.Serializable]
public struct SerializableVector3
{
    public float x;
    public float y;
    public float z;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="rX"></param>
    /// <param name="rY"></param>
    /// <param name="rZ"></param>
    public SerializableVector3(float rX, float rY, float rZ)
    {
        x = rX;
        y = rY;
        z = rZ;
    }

    /// <summary>
    /// Returns a string representation of the object
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return String.Format("[{0}, {1}, {2}]", x, y, z);
    }

    /// <summary>
    /// Automatic conversion from SerializableVector3 to Vector3
    /// </summary>
    /// <param name="rValue"></param>
    /// <returns></returns>
    public static implicit operator Vector3(SerializableVector3 rValue)
    {
        return new Vector3(rValue.x, rValue.y, rValue.z);
    }

    /// <summary>
    /// Automatic conversion from Vector3 to SerializableVector3
    /// </summary>
    /// <param name="rValue"></param>
    /// <returns></returns>
    public static implicit operator SerializableVector3(Vector3 rValue)
    {
        return new SerializableVector3(rValue.x, rValue.y, rValue.z);
    }
}






[Serializable]
public class ProjectV2
{
    public string author, name;
    public int mins, secs;
    public string source, creatorNick;

    public bool hasImage;
    public enum ImageExtension { Jpeg, Png }
    public ImageExtension imageExtension;
    public byte[] image; // has bytes only in zipped file (.bsz)

    public enum AudioExtension { Ogg, Mp3 }
    public AudioExtension audioExtension;
    public byte[] audioFile; // has bytes only in .bsz

    public List<Difficulty> difficulties;
}
[Serializable]
public class Difficulty
{
    public string name; // Selected by user
    public int stars; // Selected by user

    public float realStars; // Calculated by alg

    public List<BeatCubeClass> beatCubeList = new List<BeatCubeClass>();


    public void CalculateRealStars()
    {
        float minDelayBeforeNextCube = 9999;
        for (int i = 1; i < beatCubeList.Count - 1; i++)
        {
            BeatCubeClass prevCube = beatCubeList[i - 1];
            BeatCubeClass currentCube = beatCubeList[i];

            if(currentCube.time - prevCube.time < minDelayBeforeNextCube)
            {
                minDelayBeforeNextCube = currentCube.time - prevCube.time;
            }
        }

        // Based on min delay before next cube spawn calculate real difficulty of this Difficulty (Haha difficulty of difficulty :D)
        realStars = float.MaxValue; // XD
    }
}