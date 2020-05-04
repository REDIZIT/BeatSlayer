using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net;
using ProjectManagement;
using UnityEngine;

namespace InGame.Helpers
{
    public static class Helpers
    {
        private static WebClient c = new WebClient();
        
        public const string url_downloadMap = "http://176.107.160.146/Home/DownloadProject?trackname={0}&nickname={1}";
        
        public static void DownloadMap(string trackname, string nick, Action<DownloadProgressChangedEventArgs> progressCallback, Action<AsyncCompletedEventArgs> completeCallback)
        {
            string url = string.Format(url_downloadMap, trackname.Replace("&", "%amp%"), nick.Replace("&", "%amp%"));
            
            if (!Directory.Exists(Application.persistentDataPath + "/temp")) Directory.CreateDirectory(Application.persistentDataPath + "/temp");
            string tempPath = Application.persistentDataPath + "/temp/" + trackname + ".bsz";
            
            c = new WebClient();
            c.DownloadProgressChanged += (sender, args) =>
            {
                progressCallback(args);
            };
            c.DownloadFileCompleted += (sender, args) =>
            {
                bool doUnpack = false;
                
                if(args.Cancelled) Debug.Log("Download cancelled");
                else if (args.Error != null) Debug.LogError("Download error\n" + args.Error);
                else
                {
                    doUnpack = true;
                }

                if (doUnpack)
                {
                    ProjectManager.UnpackBspFile(tempPath);
                }
                else
                {
                    File.Delete(tempPath);
                }
                
                completeCallback(args);
                
                // -1 coz of Difficulty has no Downloads field 
                DatabaseScript.SendStatistics(trackname, nick, -1, DatabaseScript.StatisticsKeyType.Download);
            };
            
            c.DownloadFileAsync(new Uri(url), tempPath);
        }

        public static void CancelDownloading()
        {
            c.CancelAsync();
        }
    }
}