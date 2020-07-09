using GameNet;
using InGame.Settings;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace InGame.Game
{
    public class AudioManager : MonoBehaviour
    {
        public AudioSource asource;
        public AudioSource spectrumAsource;

        [Header("UI Audio")]
        public AudioSource uisource;

        public AudioClip keyPressClip, menuOpenClip;

        public GameManager gm;

        public bool sliceEffectEnabled = true;
        public float sliceEffectVolume = 1;

        //private List<int> playedIds = new List<int>();
        private int currentSoundId;


        private void Start()
        {
            sliceEffectEnabled = SettingsManager.Settings.Sound.SliceEffectEnabled;
            sliceEffectVolume = SettingsManager.Settings.Sound.SliceEffectVolume;
        }

        private void Update()
        {
            //playedIds.Clear();
        }


        public void PlaySource()
        {
            asource.Play();
            spectrumAsource.Play();
        }
        public void PauseSource()
        {
            asource.Pause();
            spectrumAsource.Pause();
        }



        public void OnKeyPress()
        {
            uisource.PlayOneShot(keyPressClip);
        }
        public void OnMenuPress()
        {
            uisource.PlayOneShot(menuOpenClip);
        }

        public void PlayCubeSlice()
        {
            if (!sliceEffectEnabled) return;

            // To prevent playing sound which already was played in this frame
            //List<int> availableIds = Payload.HitSoundIds.Where(c => !playedIds.Contains(c)).ToList();

            //int id;
            //if(availableIds.Count() > 0)
            //{
            //    id = availableIds[Random.Range(0, availableIds.Count())];
            //}
            //else
            //{
            //    // If all sounds ware played in this frame, take sound, which was played later than others
            //    id = playedIds.Last();
            //    playedIds.RemoveAt(availableIds.Count - 1);
            //}

            //playedIds.Add(id);


            currentSoundId++;
            currentSoundId = (int)Mathf.Repeat(currentSoundId, Payload.HitSoundIds.Count);

            Debug.Log("Play sound id " + currentSoundId);
            AndroidNativeAudio.play(currentSoundId, sliceEffectVolume, -1);
        }
    }
}