using UnityEngine;

namespace InGame.Extensions.Objects
{
    public static class UnityObjectsExtensions
    {
        /// <summary>
        /// Copy data from audioclip and return new clip. Fix actions with clip A affecting to clip B (heap)
        /// </summary>
        public static AudioClip Clone(this AudioClip audioClip)
        {
            AudioClip newAudioClip = AudioClip.Create(audioClip.name, audioClip.samples, audioClip.channels, audioClip.frequency, false);
            float[] copyData = new float[audioClip.samples * audioClip.channels];
            audioClip.GetData(copyData, 0);
            newAudioClip.SetData(copyData, 0);
            return newAudioClip;
        }

    }
}
