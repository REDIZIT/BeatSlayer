using Michsky.UI.ModernUIPack;
using UnityEngine;
using UnityEngine.UI;

namespace InGame.Menu.Maps
{
    public class MapsDownloadQueueItem : MonoBehaviour
    {
        public MapsDownloadQueuer queuer;
        public MapDownloadTask task;

        [Header("UI")]
        public Image background;
        public Text placeText, tracknameText, mapperText;
        public RawImage coverImage;
        public ProgressBar progressCircle;
        public GameObject completeCheckmark, completeError;
        public GameObject cancelBtn, hideQueueWindowBtn, closeWindowBtn;


        public void RefreshAsFirst(MapDownloadTask task, int tasksCount, bool spaceForCloseBtn)
        {
            this.task = task;

            placeText.text = (task.TaskPosition + 1) + "/" + tasksCount;

            Refresh();

            hideQueueWindowBtn.SetActive(true);
            closeWindowBtn.SetActive(spaceForCloseBtn);

            background.color = new Color32(55, 55, 55, 255);
        }
        public void RefreshAsRegular(MapDownloadTask task)
        {
            this.task = task;

            placeText.text = (task.TaskPosition + 1).ToString();

            Refresh();

            hideQueueWindowBtn.SetActive(false);
            closeWindowBtn.SetActive(false);

            background.color = new Color32(40, 40, 40, 255);
        }
        private void Refresh()
        {
            tracknameText.text = task.Trackname;
            mapperText.text = task.Mapper;
            coverImage.texture = task.Cover;

            cancelBtn.SetActive(task.TaskState != MapDownloadTask.State.Completed);

            progressCircle.gameObject.SetActive(task.TaskState == MapDownloadTask.State.Downloading);
            completeCheckmark.SetActive(task.TaskState == MapDownloadTask.State.Completed);
            completeError.SetActive(task.TaskState == MapDownloadTask.State.Error || task.TaskState == MapDownloadTask.State.Canceled);
        }


        private void Update()
        {
            if (task.TaskState == MapDownloadTask.State.Downloading)
            {
                completeCheckmark.SetActive(false);
                completeError.SetActive(false);

                progressCircle.gameObject.SetActive(true);
                progressCircle.CurrentPercent = task.ProgressPercentage;
            }
            else if (task.TaskState == MapDownloadTask.State.Completed)
            {
                progressCircle.gameObject.SetActive(false);

                completeCheckmark.SetActive(true);
                completeError.SetActive(false);
            }
            else if (task.TaskState == MapDownloadTask.State.Error)
            {
                progressCircle.gameObject.SetActive(false);

                completeCheckmark.SetActive(false);
                completeError.SetActive(true);
            }
        }

        public void OnProgress(int percents)
        {
            if (progressCircle == null) return;

            completeCheckmark.SetActive(false);
            completeError.SetActive(false);

            progressCircle.gameObject.SetActive(true);
            progressCircle.CurrentPercent = percents;
        }
        public void OnComplete(bool success)
        {
            if (progressCircle == null) return;

            progressCircle.gameObject.SetActive(false);

            completeCheckmark.SetActive(success);
            completeError.SetActive(!success);
        }



        public void OnCancelBtnClick()
        {
            //Destroy(gameObject);
            queuer.RemoveTask(this);
        }
    }
}
