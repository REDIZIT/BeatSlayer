using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InGame.Game
{
    public class AudioManager : MonoBehaviour
    {
        public AudioSource asource;
        public AudioSource spectrumAsource;

        public GameManager gm;





        private void Update()
        {
            
        }


        public void PlaySource()
        {
            asource.Play();
        }
        public void PauseSource()
        {
            asource.Pause();
        }
    }
}