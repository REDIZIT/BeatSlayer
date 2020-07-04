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
    }
}