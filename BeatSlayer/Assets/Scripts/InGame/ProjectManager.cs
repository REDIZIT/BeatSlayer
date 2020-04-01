using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;
using UnityEngine;

namespace ProjectManagement
{
    public static class ProjectManager
    {
        public static Project LoadProject(string projectPath)
        {
            XmlSerializer xml = new XmlSerializer(typeof(Project));
            using(var s = File.OpenRead(projectPath))
            {
                return (Project)xml.Deserialize(s);
            }
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
            Debug.Log($"LoadAudioCoroutine({path})");
            Debug.Log($"AudioFiles exists? " + (File.Exists(path)));
            // Must work in sync mode!
            using (WWW www = new WWW("file:///" + path))
            {
                yield return www;
                LoadingData.aclip = www.GetAudioClip();
            }
        }
        public static Sprite LoadCover(string path)
        {
            return LoadSprite(path);
        }

        public static Texture2D LoadTexure(byte[] bytes)
        {
            Texture2D tex = new Texture2D(0, 0);
            tex.LoadImage(bytes);
            return tex;
        }
        public static Sprite LoadSprite(string path)
        {
            byte[] bytes = File.ReadAllBytes(path);
            Texture2D tex = new Texture2D(0, 0);
            tex.LoadImage(bytes);
            return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
        }
    }
}