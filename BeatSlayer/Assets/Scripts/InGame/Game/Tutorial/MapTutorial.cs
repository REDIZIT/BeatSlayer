using System.Collections.Generic;
using UnityEngine;

namespace InGame.Game.Tutorial
{
    public class MapTutorial : MonoBehaviour
    {
        public AudioSource asource;
        public bool IsEnabled { get; set; }

        public List<AudioTimeCode> timecodes;

        public AudioClip[] cubeHitClips, bombHitClips;

        private void Update()
        {
            if (!IsEnabled) return;

            if(timecodes.Count > 0)
            {
                if(asource.time >= timecodes[0].time)
                {
                    asource.PlayOneShot(timecodes[0].clip);
                    timecodes.RemoveAt(0);
                }
            }
        }

        public void OnBombHit()
        {
            if (!IsEnabled) return;

            asource.PlayOneShot(bombHitClips[Random.Range(0, bombHitClips.Length)]);
        }
    }

    [System.Serializable]
    public class AudioTimeCode
    {
        public AudioClip clip;

        /// <summary>
        /// Time in seconds
        /// </summary>
        public float time;
    }
}
