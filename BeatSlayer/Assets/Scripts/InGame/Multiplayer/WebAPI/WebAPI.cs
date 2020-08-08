using System;
using System.Collections;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using BeatSlayer.Utils;
using GameNet;
using Multiplayer.Accounts;
using Newtonsoft.Json;
using ProjectManagement;
using UnityEngine;
using UnityEngine.Networking;
using Debug = UnityEngine.Debug;

namespace Web
{
    public static class WebAPI
    {
        public static string Apibase => NetCore.Url_Server;

        public const string url_uploadAvatar = "/WebAPI/UploadAvatar";
        public const string url_getAvatar = "/WebAPI/GetAvatar?nick={0}";
        public const string url_uploadBackground = "/WebAPI/UploadBackground?nick={0}";        
        public const string url_getBackground = "/WebAPI/GetBackground?nick={0}";
        public const string url_getTutorialGroup = "/Database/GetTutorialGroup";
        public const string url_onGameLaunch = "/Account/OnGameLaunch";
        public const string url_onGameLaunchAnonim = "/Account/OnGameLaunch?anonim=true";
        public const string url_onMapPlayed = "/Account/OnMapPlayed?approved={0}";
        public static string url_downloadProject = Apibase + "/Maps/Download?trackname={0}&nick={1}";

        private static WebClient mapDownloadClient;




        public static void DownloadMap(string trackname, string nick, Action<DownloadProgressChangedEventArgs> progressCallback, Action<AsyncCompletedEventArgs> completeCallback)
        {
            string url = string.Format(url_downloadProject, trackname.Replace("&", "%amp%"), nick.Replace("&", "%amp%"));

            Directory.CreateDirectory(Application.persistentDataPath + "/temp");
            string tempPath = Application.persistentDataPath + "/temp/" + trackname + ".bsz";

            mapDownloadClient = new WebClient();
            mapDownloadClient.DownloadProgressChanged += (sender, args) =>
            {
                if (!Application.isPlaying) CancelMapDownloading();
                else progressCallback(args);
            };
            mapDownloadClient.DownloadFileCompleted += (sender, args) =>
            {
                if (!Application.isPlaying)
                {
                    return;
                }

                bool doUnpack = false;

                if (args.Cancelled) Debug.Log("Download cancelled");
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

            mapDownloadClient.DownloadFileAsync(new Uri(url), tempPath);
        }
        public static void CancelMapDownloading()
        {
            mapDownloadClient?.CancelAsync();
        }




        public static void UploadAvatar(string nick, Texture2D tex, Action<OperationMessage> callback)
        {

            //tex.Resize(300, 300);
            //tex.Apply();
            TextureScale.Bilinear(tex, 300, 300);

            byte[] bytes = tex.EncodeToPNG();


            string url = Apibase + string.Format(url_uploadAvatar, nick);
            SendFile(nick, url, bytes, "avatar.png", callback);

            //string url = apibase + url_uploadAvatar;
            //Upload(url, nick, bytes, callback);

            //string url = apibase + url_uploadAvatar;
            //UploadImageAsync(url, bytes, "123.jpg");
        }
        public static void UploadBackground(string nick, Texture2D tex, Action<OperationMessage> callback)
        {
            byte[] bytes = tex.EncodeToPNG();

            string url = Apibase + string.Format(url_uploadBackground, nick);
            SendFile(nick, url, bytes, "background.png", callback);

            //string url = apibase + url_uploadBackground;
            //Upload(url, nick, bytes, callback);
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
                string url = Apibase + string.Format(url_getAvatar, nick);
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
                string url = Apibase + string.Format(url_getBackground, nick);
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
            if (Payload.Account.Nick == nick)
            {
                if (File.Exists(filepath))
                {
                    return File.ReadAllBytes(filepath);
                }

                return new byte[0];
            }
            return null;
        }


        public static void OnGameLaunch()
        {
            if (Application.internetReachability == NetworkReachability.NotReachable) return;

            bool hasOldSessionFile = AccountUI.HasSession(true);
            bool hasNewSessionFile = AccountUI.HasSession(false);

            WebClient c = new WebClient();
            Uri url;

            if (hasOldSessionFile || hasNewSessionFile) url = new Uri(Apibase + url_onGameLaunch);
            else url = new Uri(Apibase + url_onGameLaunchAnonim);

            c.DownloadDataAsync(url);
        }
        public static void OnMapPlayed(bool approved)
        {
            WebClient c = new WebClient();
            Uri url = new Uri(string.Format(Apibase + url_onMapPlayed, approved));

            c.DownloadDataAsync(url);
        }



        public static void Upload(string url, string nick, byte[] bytes, Action<OperationMessage> callback)
        {
            //UnityMainThreadDispatcher.Instance().Enqueue(IEUpload);
            
            /*UnityWebRequest www = UnityWebRequest.Put(url, bytes);
            www.SendWebRequest();*/


            WWWForm form = new WWWForm();

            //form.headers.Clear();
            /*form.headers["Content-Type"] = "multipart/form-data";

            form.AddField("nick", nick);
            form.AddBinaryData("file", bytes);*/

            FileDto dto = new FileDto()
            {
                Nick = nick,
                File = bytes
            };

            string json = JsonConvert.SerializeObject(dto);

            form.AddField("json", json);

            //WWW w = new WWW(url, Encoding.UTF8.GetBytes(json));

            json = System.Net.WebUtility.UrlEncode(json);
            string _url = string.Format("https://www.bsserver.tk/WebAPI/UploadAvatar?json={0}", json);
            Debug.Log(_url);


            WebClient c = new WebClient();
            c.DownloadString(_url);



            /*while(w.isDone)


            if (!string.IsNullOrEmpty(w.error))
            {
                Debug.Log(w.error);
            }
            else
            {
                Debug.Log(w.text);
            
            }*/
        }
            
        public static IEnumerator IEUpload(string url, byte[] bytes, Action<OperationMessage> callback)
        {
            UnityWebRequest www = UnityWebRequest.Put(url, bytes);
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {
                Debug.Log(www.error);
            }
            else
            {
                Debug.Log("Upload complete!");
            }
        }

        static void SendFile(string nick, string url, byte[] bytes, string filename, Action<OperationMessage> callback)
        {
            /*ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
            ServicePointManager.ServerCertificateValidationCallback =
                new System.Net.Security.RemoteCertificateValidationCallback((s, c, ch, ss) =>
                {
                    return true;
                });*/


            CI.HttpClient.HttpClient client = new CI.HttpClient.HttpClient();
            
            var httpContent = new CI.HttpClient.MultipartFormDataContent();

            httpContent.Add(new CI.HttpClient.StringContent(nick), "nick");

            CI.HttpClient.ByteArrayContent content = new CI.HttpClient.ByteArrayContent(bytes, "multipart/form-data");
            httpContent.Add(content, "file", filename);

            client.Post(new Uri(url), httpContent, CI.HttpClient.HttpCompletionOption.AllResponseContent, (r) =>
            {
                string json = r.ReadAsString();
                Debug.Log(url + " => " + json);
                OperationMessage msg = JsonConvert.DeserializeObject<OperationMessage>(json);
                callback(msg);
            });
        }


        static async Task UploadImageAsync(string url, byte[] bytes, string fileName)
        {
            /*HttpContent fileStreamContent = new StreamContent(image);
            fileStreamContent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data") { Name = "file", FileName = fileName };*/
            HttpContent fileContent = new ByteArrayContent(bytes);
            //fileStreamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("multipart/form-data");
            using (var client = new HttpClient())
            using (var formData = new MultipartFormDataContent())
            {
                formData.Add(fileContent);
                var response = await client.PostAsync(url, formData);

                Debug.Log("Response is " + JsonConvert.SerializeObject(response, Formatting.Indented));
                //return response.IsSuccessStatusCode;
            }
        }
    }

    class FileDto
    {
        public string Nick { get; set; }
        public byte[] File { get; set; }
    }
}