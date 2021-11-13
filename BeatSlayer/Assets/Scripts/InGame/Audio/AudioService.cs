using InGame.Settings;
using UnityEngine;

namespace InGame.Audio
{
	[RequireComponent(typeof(AudioSource))]
	public class AudioService : MonoBehaviour
	{
		private AudioSource source;

        private void Awake()
        {
            source = GetComponent<AudioSource>();
        }

        public void PlayOneShot(AudioClip clip)
        {
            source.PlayOneShot(clip);
        }
    }
}