using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Web
{
    public static class WebAPI
    {
        //public const string apibase = "https://localhost:5001";
        public const string apibase = "http://www.bsserver.tk";
        public const string url_uploadAvatar = "/WebAPI/UploadAvatar?nick={0}";
        public const string url_getAvatar = "/WebAPI/GetAvatar?nick={0}";
        public const string url_uploadBackground = "/WebAPI/UploadBackground?nick={0}";        
        public const string url_getBackground = "/WebAPI/GetBackground?nick={0}";

        public static void UploadAvatar(string nick, string filename, Action<OperationMessage> callback)
        {
            byte[] bytes = File.ReadAllBytes(filename);
            string url = apibase + string.Format(url_uploadAvatar, nick);
            SendFile(nick, url, bytes, Path.GetFileName(filename), callback);
        }
        public static void UploadBackground(string nick, string filename, Action<OperationMessage> callback)
        {
            byte[] bytes = File.ReadAllBytes(filename);
            string url = apibase + string.Format(url_uploadBackground, nick);
            SendFile(nick, url, bytes, Path.GetFileName(filename), callback);
        }
        

        public static void GetAvatar(string nick, Action<byte[]> callback)
        {
            string url = apibase + string.Format(url_getAvatar, nick);
            WebClient c = new WebClient();
            c.DownloadDataCompleted += (sender, args) =>
            {
                callback(args.Result);
            };
            c.DownloadDataAsync(new Uri(url));
        }
        public static void GetBackground(string nick, Action<byte[]> callback)
        {
            string url = apibase + string.Format(url_getBackground, nick);
            WebClient c = new WebClient();
            c.DownloadDataCompleted += (sender, args) =>
            {
                callback(args.Result);
            };
            c.DownloadDataAsync(new Uri(url));
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
            /*await Task.Factory.StartNew(() =>
            {
                
            });*/
        }
    }
}