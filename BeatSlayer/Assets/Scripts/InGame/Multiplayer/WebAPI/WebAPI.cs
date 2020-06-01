using System;
using System.Collections;
using System.IO;
using System.Net;
using GameNet;
using Newtonsoft.Json;
using ProjectManagement;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Web
{
    public static class WebAPI
    {
        public static string apibase
        {
            get { return NetCore.Url_Server; }
        }

        public const string url_uploadAvatar = "/WebAPI/UploadAvatar?nick={0}";
        public const string url_getAvatar = "/WebAPI/GetAvatar?nick={0}";
        public const string url_uploadBackground = "/WebAPI/UploadBackground?nick={0}";        
        public const string url_getBackground = "/WebAPI/GetBackground?nick={0}";
        public const string url_getTutorialGroup = "/Database/GetTutorialGroup";

        


        public static void UploadAvatar(string nick, Texture2D tex, Action<OperationMessage> callback)
        {
            byte[] bytes = tex.EncodeToPNG();
            string url = apibase + string.Format(url_uploadAvatar, nick);
            SendFile(nick, url, bytes, "avatar.png", callback);
        }
        public static void UploadBackground(string nick, Texture2D tex, Action<OperationMessage> callback)
        {
            byte[] bytes = tex.EncodeToPNG();
            string url = apibase + string.Format(url_uploadBackground, nick);
            SendFile(nick, url, bytes, "avatar.png", callback);
        }





        public static void GetAvatar(string nick, Action<Texture2D> callback)
        {
            GetAvatar(nick, (byte[] bytes) =>
            {
                callback(ProjectManager.LoadTexture(bytes));
            });
        }
        public static void GetAvatar(string nick, Action<byte[]> callback, bool forceUpdate = false)
        {
            string filepath = Application.persistentDataPath + "/data/account/avatar.pic";
            byte[] bytes = LoadImage(filepath, nick);
            
            if (forceUpdate || bytes == null || bytes.Length == 0)
            {
                string url = apibase + string.Format(url_getAvatar, nick);
                WebClient c = new WebClient();
                c.DownloadDataCompleted += (sender, args) =>
                {
                    // No file but own account
                    if (forceUpdate || bytes != null)
                    {
                        File.WriteAllBytes(filepath, args.Result);
                    }
                    callback(args.Result);
                };
                c.DownloadDataAsync(new Uri(url));
            }
            else callback(bytes);
        }





        public static void GetBackground(string nick, Action<byte[]> callback, bool forceUpdate = false)
        {
            string filepath = Application.persistentDataPath + "/data/account/background.pic";
            byte[] bytes = LoadImage(filepath, nick);

            if (forceUpdate || bytes == null || bytes.Length == 0)
            {
                string url = apibase + string.Format(url_getBackground, nick);
                WebClient c = new WebClient();
                c.DownloadDataCompleted += (sender, args) =>
                {
                    // No file but own account
                    if (forceUpdate || bytes != null)
                    {
                        File.WriteAllBytes(filepath, args.Result);
                    }

                    callback(args.Result);
                };
                c.DownloadDataAsync(new Uri(url));
            }
            else callback(bytes);
        }

        // Load cached images of own account
        static byte[] LoadImage(string filepath, string nick)
        {
            if (NetCorePayload.CurrentAccount.Nick == nick)
            {
                if (File.Exists(filepath))
                {
                    return File.ReadAllBytes(filepath);
                }

                return new byte[0];
            }
            return null;
        }




        public static string GetTutorialGroup()
        {
            return new WebClient().DownloadString(apibase + url_getTutorialGroup);
        }








        static async void SendFile(string nick, string url, byte[] bytes, string filename, Action<OperationMessage> callback)
        { 
            CI.HttpClient.HttpClient client = new CI.HttpClient.HttpClient();
            
            var httpContent = new CI.HttpClient.MultipartFormDataContent();

            httpContent.Add(new CI.HttpClient.StringContent(nick), "nick");
            //httpContent.Add(new CI.HttpClient.StringContent(LegacyAccount.password), "password");

            CI.HttpClient.ByteArrayContent content = new CI.HttpClient.ByteArrayContent(bytes, "multipart/form-data");
            httpContent.Add(content, "file", filename);

            //httpContent.Add(new CI.HttpClient.StringContent(Path.GetExtension(filename)), "extension");

            client.Post(new System.Uri(url), httpContent, CI.HttpClient.HttpCompletionOption.AllResponseContent, (r) =>
            {
                string json = r.ReadAsString();
                Debug.Log(json);
                OperationMessage msg = JsonConvert.DeserializeObject<OperationMessage>(json);
                callback(msg);
            });
        }
    }
}