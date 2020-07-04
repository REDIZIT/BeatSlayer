﻿using InGame.SceneManagement;
using ProjectManagement;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace InGame.Menu
{
    public class PracticeModeUI : MonoBehaviour
    {
        public GameObject overlay;
        public Slider startTimeSlider, musicSpeedSlider, cubesSpeedSlider;

        // Костыль! Нужен только для получения времени
        //private Project selectedProject;
        private AudioClip selectedClip;

        private MapInfo mapInfo;
        private DifficultyInfo difficultyInfo;

        public void ShowWindow(MapInfo mapInfo, DifficultyInfo difficultyInfo)
        {
            this.mapInfo = mapInfo;
            this.difficultyInfo = difficultyInfo;

            overlay.SetActive(true);
            startTimeSlider.value = 0;
            musicSpeedSlider.value = 100;
            cubesSpeedSlider.value = 100;

            string perPath = Application.persistentDataPath;

            string trackname = mapInfo.author + "-" + mapInfo.name;
            string filepath = perPath + "/maps/" + trackname + "/" + mapInfo.nick + "/" + trackname + ".mp3";
            if (!File.Exists(filepath)) filepath = Path.ChangeExtension(filepath, ".ogg");

            selectedClip = ProjectManager.LoadAudio(filepath);

            //startTimeSlider.maxValue = selectedProject.secs;
            startTimeSlider.maxValue = selectedClip.length;
        }

        public void OnPracticeBtnClick()
        {
            // !!!!!  Set defaults coz of mods  !!!!!
            SSytem.instance.SetInt("CubesSpeed", 10);
            SSytem.instance.SetInt("MusicSpeed", 10);


            var parameters = SceneloadParameters.AuthorMusicPreset(mapInfo, difficultyInfo, 
                startTimeSlider.value, musicSpeedSlider.value / 100f, cubesSpeedSlider.value / 100f);

            SceneController.instance.LoadScene(parameters);
        }
    }
}