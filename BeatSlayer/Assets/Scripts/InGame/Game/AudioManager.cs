using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InGame.Game
{
    public class AudioManager : MonoBehaviour
    {
        public AudioSource asource;
        public AudioSource spectrumAsource;

        public void SetClip(AudioClip clip)
        {
            asource.clip = clip;
        }

        //int startFrame = 0;
        //private void Awake()
        //{
        //    startFrame = Time.frameCount;
        //}
        public void PlaySource()
        {
            //Time.timeScale = 0;
            //Debug.Log("Frames passed: " + (Time.frameCount - startFrame));
            asource.Play();
        }
        public void PauseSource()
        {
            asource.Pause();
        }



        public void PlaySpectrumSource()
        {
            spectrumAsource.Play();
        }
        public void PauseSpectrumSource()
        {
            spectrumAsource.Pause();
        }



        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                PlaySource();
            }
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                PlaySpectrumSource();
            }
        }
    }
}