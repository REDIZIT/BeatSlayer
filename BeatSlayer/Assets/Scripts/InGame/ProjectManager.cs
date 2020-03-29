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
            // Must work in sync mode!
            using (WWW www = new WWW("file:///" + System.Net.WebUtility.UrlEncode(path)))
            {
                while(!www.isDone) { Thread.Sleep(20); }
                return www.GetAudioClip(false, false);
            }
        }
    }
}