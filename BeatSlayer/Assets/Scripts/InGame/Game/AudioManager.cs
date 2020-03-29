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
            Debug.Log("[AUDIO MANAGER] SetClip");
            asource.clip = clip;
        }

        public void PlaySource()
        {
            Debug.Log("Play asource");
            asource.Play();
        }
        public void PauseSource()
        {
            Debug.Log("Pause asource");
            asource.Pause();
        }



        public void PlaySpectrumSource()
        {
            Debug.Log("Play spectrumAsource");
            spectrumAsource.Play();
        }
        public void PauseSpectrumSource()
        {
            Debug.Log("Pause spectrum source");
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