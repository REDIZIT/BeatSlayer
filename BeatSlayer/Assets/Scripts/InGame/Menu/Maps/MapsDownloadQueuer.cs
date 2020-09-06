using CoversManagement;
using InGame.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Web;

namespace InGame.Menu.Maps
{
    public static class MapsDownloadQueuerBackground
    {
        public static List<MapDownloadTask> queue = new List<MapDownloadTask>();
        public static List<MapDownloadTask> waitingQueue => 
            queue.Where(c => c.TaskState == MapDownloadTask.State.Waiting).ToList();
        public static List<MapDownloadTask> uncompletedQueue => 
            queue.Where(c => c.TaskState == MapDownloadTask.State.Waiting || c.TaskState == MapDownloadTask.State.Downloading).ToList();

        public static MapsDownloadQueueItem ActiveItem { get; set; }

        public static Action OnTaskCompletedCallback;




        public static MapDownloadTask AddTask(string trackname, string mapper)
        {
            MapDownloadTask task = new MapDownloadTask(trackname, mapper, queue.Count);
            queue.Add(task);

            if (queue.All(c => c.TaskState != MapDownloadTask.State.Downloading))
            {
                StartDownloading(task);
            }

            return task;
        }
        public static void RemoveTask(MapDownloadTask task)
        {
            bool isDownloading = task.TaskState == MapDownloadTask.State.Downloading;

            task.TaskState = MapDownloadTask.State.Canceled;
            queue.Remove(task);

            var sorted = queue.OrderBy(c => c.TaskPosition).ToList();
            for (int i = 0; i < sorted.Count; i++)
            {
                sorted[i].TaskPosition = i;
            }



            if (isDownloading)
            {
                WebAPI.CancelMapDownloading();
                OnTaskCompleted();
            }
        }
        public static void OnTaskCompleted()
        {
            Debug.Log("On task completed");

            if (waitingQueue.Count() > 0)
            {
                StartDownloading(waitingQueue.First());
            }

            OnTaskCompletedCallback?.Invoke();
        }
        public static void StartDownloading(MapDownloadTask task)
        {
            Debug.Log("Start downloading");
            if (task.TaskState != MapDownloadTask.State.Waiting) return;

            task.TaskState = MapDownloadTask.State.Downloading;
            

            WebAPI.DownloadMap(task.Trackname, task.Mapper,
            (progressArgs) =>
            {
                if (task.TaskState == MapDownloadTask.State.Canceled) return;

                task.ProgressPercentage = progressArgs.ProgressPercentage;

                task.OnProgress?.Invoke(task.ProgressPercentage);
                ActiveItem?.OnProgress(progressArgs.ProgressPercentage);

            }, (completeArgs) =>
            {
                if (task.TaskState == MapDownloadTask.State.Canceled) return;


                task.TaskState = completeArgs.Error == null ? MapDownloadTask.State.Completed : MapDownloadTask.State.Error;

                if (task.TaskState == MapDownloadTask.State.Completed) task.OnDownloaded?.Invoke();

                ActiveItem?.OnComplete(task.TaskState == MapDownloadTask.State.Completed);

                OnTaskCompleted();
            });
        }
    }









    public class MapsDownloadQueuer : MonoBehaviour
    {
        

        [Header("Components")]
        [SerializeField] private TrackListUI listUI;
        [SerializeField] private Animator animator;

        [Header("UI")]
        public Transform content;
        public GameObject hideImage, closeImage;
        [SerializeField] private MapsDownloadQueueItem headerItem;

        public MapDownloadTask ActiveTask => MapsDownloadQueuerBackground.ActiveItem?.task;


        private bool isExpanded;
        private List<MapsDownloadQueueItem> items = new List<MapsDownloadQueueItem>();





        private void Start()
        {
            MapsDownloadQueuerBackground.OnTaskCompletedCallback = OnTaskCompleted;

            // Creating items after scene reloading
            if(MapsDownloadQueuerBackground.uncompletedQueue.Count > 0)
            {
                MapsDownloadQueuerBackground.queue.RemoveAll(
                    c => c.TaskState == MapDownloadTask.State.Completed || c.TaskState == MapDownloadTask.State.Error);

                int i = 0;
                foreach (var task in MapsDownloadQueuerBackground.uncompletedQueue)
                {
                    task.TaskPosition = i;
                    AddItem(task);
                    i++;
                }
                MapsDownloadQueuerBackground.ActiveItem = items.FirstOrDefault(c => c.task.TaskState == MapDownloadTask.State.Downloading);

                OnQueueChange();

                ShowWindow();
            }
        }


        public MapDownloadTask AddTask(string trackname, string mapper)
        {
            MapDownloadTask task = MapsDownloadQueuerBackground.AddTask(trackname, mapper);
            AddItem(task);
            MapsDownloadQueuerBackground.ActiveItem = items.First();


            OnQueueChange();

            if (MapsDownloadQueuerBackground.queue.Count == 1)
            {
                ShowWindow();
            }

            return task;
        }
        public void RemoveTask(MapsDownloadQueueItem item)
        {
            items.Remove(item);
            MapsDownloadQueuerBackground.RemoveTask(item.task);

            OnQueueChange();

            if (MapsDownloadQueuerBackground.queue.Count == 0)
            {
                OnCloseBtnClick();
            }
        }


        public void OnExpandBtnClick()
        {
            animator.Play(isExpanded ? "Collapse" : "Expand");
            isExpanded = !isExpanded;
        }
        public void OnCloseBtnClick()
        {
            StartCoroutine(CloseWindow());
        }


        private void OnTaskCompleted()
        {
            if (listUI.gameObject != null)
            {
                listUI.ReloadDownloadedList();
            }
            

            OnQueueChange();
        }





        private void OnQueueChange()
        {
            // Update expand/collapse button image
            closeImage.gameObject.SetActive(MapsDownloadQueuerBackground.uncompletedQueue.Count() == 0);


            MapsDownloadQueuerBackground.queue = MapsDownloadQueuerBackground.queue.OrderByDescending(c => c.TaskState == MapDownloadTask.State.Waiting || c.TaskState == MapDownloadTask.State.Downloading).ToList();

            int i = -1;

            foreach (var item in items)
            {
                i++;
                //MapDownloadTask task = item.task;
                var task = MapsDownloadQueuerBackground.queue[i];

                if (i == 0) item.RefreshAsFirst(task, MapsDownloadQueuerBackground.queue.Count, closeImage.activeSelf);
                else item.RefreshAsRegular(task);
            }

            MapsDownloadQueuerBackground.ActiveItem = items.FirstOrDefault();

            if (!isExpanded && MapsDownloadQueuerBackground.queue.All(c => c.TaskState == MapDownloadTask.State.Completed))
            {
                OnCloseBtnClick();
            }
        }
        private void AddItem(MapDownloadTask task)
        {
            if (task.TaskPosition == 0)
            {
                HelperUI.ClearContent(content);
                
                headerItem.OnProgress(0);
                items.Add(headerItem);

                headerItem.RefreshAsFirst(task, MapsDownloadQueuerBackground.queue.Count, false);

                if(task.Cover == null)
                {
                    CoversManager.AddPackage(new CoverRequestPackage(headerItem.coverImage, task.Trackname, task.Mapper, true, (cover) =>
                    {
                        task.Cover = cover;
                    }));
                }
                else
                {
                    headerItem.coverImage.texture = task.Cover;
                }
            }
            else
            {
                HelperUI.AddContent<MapsDownloadQueueItem>(content, (item) =>
                {
                    item.OnProgress(0);
                    items.Add(item);

                    item.RefreshAsRegular(task);


                    if (task.Cover == null)
                    {
                        CoversManager.AddPackage(new CoverRequestPackage(item.coverImage, task.Trackname, task.Mapper, true, (cover) =>
                        {
                            task.Cover = cover;
                        }));
                    }
                    else
                    {
                        item.coverImage.texture = task.Cover;
                    }
                });
            }
        }



        private IEnumerator CloseWindow()
        {
            MapsDownloadQueuerBackground.queue.Clear();
            items.Clear();

            animator.Play(isExpanded ? "Close-Collapse" : "Close-Expand");
            isExpanded = false;

            yield return new WaitForSeconds(0.4f);
            HelperUI.ClearContent(content);
        }
        private void ShowWindow()
        {
            animator.Play("Show");
        }
    }

    [Serializable]
    public class MapDownloadTask
    {
        public string Trackname { get { return trackname; } set { trackname = value; } }
        private string trackname;

        public string Mapper { get; set; }
        public Texture2D Cover { get; set; }

        public State TaskState { get { return state; } set { state = value; } }
        private State state;

        public int TaskPosition { get { return pos; } set { pos = value; } }
        private int pos;

        public int ProgressPercentage { get; set; }

        public Action<int> OnProgress { get; set; }
        public Action OnDownloaded { get; set; }


        public enum State
        {
            Waiting, Downloading, Completed, Error, Canceled
        }

        public MapDownloadTask(string trackname, string mapper, int taskPosition)
        {
            Trackname = trackname;
            Mapper = mapper;
            TaskPosition = taskPosition;

            TaskState = State.Waiting;
        }
    }
}
