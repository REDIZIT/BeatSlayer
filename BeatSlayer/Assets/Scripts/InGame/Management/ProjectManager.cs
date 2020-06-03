using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;
using UnityEngine;

namespace ProjectManagement
{
    public static class ProjectManager
    {
        public static Sprite defaultTrackSprite;
        public static Texture2D defaultTrackTexture;

        public static Project LoadProject(string projectPath)
        {
            XmlSerializer xml = new XmlSerializer(typeof(Project));
            using(var s = File.OpenRead(projectPath))
            {
                return (Project)xml.Deserialize(s);
            }
        }
        public static void DeleteProject(string trackname, string nick)
        {
            string trackPath = Application.persistentDataPath + "/maps/" + trackname;
            string mapPath = trackPath + "/" + nick;

            Directory.Delete(mapPath, true);

            if(Directory.GetDirectories(trackPath).Length == 0)
            {
                Directory.Delete(trackPath);
            }
        }
        public static bool IsMapDownloaded(string author, string name, string nick)
        {
            string trackname = author + "-" + name;
            string path = Application.persistentDataPath + "/maps/" + trackname + "/" + nick + "/" + trackname + ".bsu";
            return File.Exists(path);
        }
        public static Project UnpackBspFile(string tempPath)
        {
            Project proj;

            XmlSerializer xml = new XmlSerializer(typeof(Project));
            FileStream loadStream = File.OpenRead(tempPath);
            proj = (Project)xml.Deserialize(loadStream);
            loadStream.Close();

            string targetFolder = Application.persistentDataPath + "/maps/" + (proj.author.Trim() + "-" + proj.name.Trim()) + "/" + proj.creatorNick.Trim();
            string targetFilesPath = targetFolder + "/" + (proj.author.Trim() + "-" + proj.name.Trim());

            if (!Directory.Exists(targetFolder)) Directory.CreateDirectory(targetFolder);

            // Unpack audio file
            string audioPath = targetFilesPath + (proj.audioExtension == Project.AudioExtension.Mp3 ? ".mp3" : ".ogg");
            File.WriteAllBytes(audioPath, proj.audioFile);
            proj.audioFile = null;

            // Unpack image file
            if (proj.hasImage)
            {
                string imagePath = targetFilesPath + (proj.imageExtension == Project.ImageExtension.Jpeg ? ".jpg" : ".png");
                File.WriteAllBytes(imagePath, proj.image);
                proj.image = null;
            }

            // Saving unpacked file (in .bsu) into target folder
            Stream saveStream = File.Create(targetFilesPath + ".bsu");
            xml.Serialize(saveStream, proj);
            saveStream.Close();

            File.Delete(tempPath);

            return proj;
        }
        
        
        
        
        
        public static AudioClip LoadAudio(string path)
        {
            Debug.Log($"LoadAudio({path})");
            // Must work in sync mode!
            using (WWW www = new WWW("file:///" + System.Net.WebUtility.UrlEncode(path)))
            {
                while(!www.isDone) { Thread.Sleep(16); }
                return www.GetAudioClip(false, false);
            }
        }
        public static IEnumerator LoadAudioCoroutine(string path)
        {
            // Must work in sync mode!
            using (WWW www = new WWW("file:///" + path))
            {
                yield return www;
                LoadingData.aclip = www.GetAudioClip();
            }
        }
        public static IEnumerator LoadAudioCoroutine(string path, Action<AudioClip> callback)
        {
            // Must work in unity main thread!
            using (WWW www = new WWW("file:///" + path))
            {
                yield return www;
                Debug.Log("Clip loaded");
                callback(www.GetAudioClip());
            }
        }
        
        
        
        
        
        public static Texture2D LoadCover(string trackname, string nick)
        {
            string groupFolder = Application.persistentDataPath + "/maps/" + trackname;
            if (!Directory.Exists(groupFolder)) return defaultTrackTexture;

            string mapFolder = groupFolder + "/" + nick;
            if (!Directory.Exists(mapFolder)) return defaultTrackTexture;

            string path = GetCoverPath(trackname, nick);
            if (path == "") return defaultTrackTexture;

            return ProjectManager.LoadTexture(path);
        }


        public static Texture2D LoadTexture(string path)
        {
            return LoadTexture(File.ReadAllBytes(path));
        }
        public static Texture2D LoadTexture(byte[] bytes)
        {
            Texture2D tex = new Texture2D(0, 0);
            tex.LoadImage(bytes);
            return tex;
        }
        public static Sprite LoadSprite(string path)
        {
            byte[] bytes = File.ReadAllBytes(path);
            return LoadSprite(bytes);
        }
        public static Sprite LoadSprite(byte[] bytes)
        {
            Texture2D tex = new Texture2D(0, 0);
            tex.LoadImage(bytes);
            return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
        }
        
        
        public static string GetCoverPath(string trackname, string nick)
        {
            string groupFolder = Application.persistentDataPath + "/maps/" + trackname;
            if (!Directory.Exists(groupFolder)) return "";

            string mapFolder = groupFolder + "/" + nick;
            if (nick == "")
            {
                mapFolder = Directory.GetDirectories(groupFolder)[0];
            }

            string jpgPath = mapFolder + "/" + trackname + ".jpg";
            string pngPath = mapFolder + "/" + trackname + ".png";
            if (File.Exists(jpgPath)) return jpgPath;
            else if (File.Exists(pngPath)) return pngPath;

            return "";
        }
        
        public static string GetRealPath(string pathWithoutExtension, params string[] extensions)
        {
            foreach (var ext in extensions)
            {
                string extension = ext;
                if (!extension.Contains(".")) extension = "." + extension;

                string path = pathWithoutExtension + extension;
                if (File.Exists(path))
                {
                    return path;
                }
            }

            return "";
        }
    }

    
    
    
    
    
    public class GroupInfo
    {
        public string author, name;
        public int mapsCount;
        
        public GroupType groupType;
        public enum GroupType
        {
            Author, Own
        }
    }
    public class GroupInfoExtended : GroupInfo
    {
        public int allDownloads, allPlays, allLikes, allDislikes;

        public DateTime updateTime;
        public bool IsNew
        {
            get
            {
                return (DateTime.Now - updateTime).TotalDays <= 3;
            }
        }

        public List<string> nicks;
        public string filepath; // Path to file on phone. Used only for Own music
    }
    public class MapInfo
    {
        public GroupInfo group;
        public bool isMapDeleted;

        public string author { get { return group.author; } }
        public string name { get { return group.name; } }

        public string nick;

        /// <summary>
        /// Deprecated. Use Likes, Dislikes, PlayCount and downloads (not deprecated)
        /// </summary>
        public int likes, dislikes, playCount, downloads;

        public int Downloads { get { return downloads; } }
        public int PlayCount { get { return difficulties.Sum(c => c.playCount); } }
        public int Likes { get { return difficulties.Sum(c => c.likes); } }
        public int Dislikes { get { return difficulties.Sum(c => c.dislikes); } }

        public string difficultyName;
        public int difficultyStars;
        public List<DifficultyInfo> difficulties;
        

        public DateTime publishTime;

        public bool approved;
        public DateTime grantedTime;

        // If map isn't on server
        [JsonIgnore] public string filepath = "";

        public bool IsGrantedNow
        {
            get
            {
                if (!approved) return false;
                else return grantedTime > publishTime;
            }
        }

        public MapInfo() { }
        public MapInfo(GroupInfo group)
        {
            this.group = group;
        }
    }

    public class DifficultyInfo
    {
        public string name;
        public int stars;
        public int id = -1;

        public int downloads, playCount, likes, dislikes;
    }
    
    public static class ProjectUpgrader
    {
        /// <summary>
        /// Update project into project with difficulties system (date by 25.04.2020)
        /// </summary>
        public static Project UpgradeToDifficulty(Project legacyProject)
        {
            Debug.Log("UpgradeToDifficulty " + legacyProject.author + "-" + legacyProject.name);
            Debug.Log("Upgrade ls count: " + legacyProject.beatCubeList.Count);
            Project proj = legacyProject;
    
            proj.difficulties = new List<Difficulty>();
            proj.difficulties.Add(new Difficulty()
            {
                name = legacyProject.difficultName,
                stars = legacyProject.difficultStars, 
                id = 0
            });
            proj.difficulties[0].beatCubeList.AddRange(legacyProject.beatCubeList);

            foreach (var cls in proj.difficulties[0].beatCubeList)
            {
                cls.speed = 1;
                if (cls.type == BeatCubeClass.Type.Line)
                {
                    if (cls.linePoints.Count > 0)
                    {
                        cls.lineEndRoad = cls.road;
                        cls.lineLenght = cls.linePoints[1].z;
                        cls.lineEndLevel = cls.level;
                        cls.linePoints.Clear();
                    }
                }
            }
    
            
            return proj;
        }
    }
}