using UnityEngine;

namespace InGame.Animations
{
    [RequireComponent(typeof(ParticleSystem))]
    public class FireworksSystem : MonoBehaviour
    {
        public ParticleSystem ps;

        public AudioSource asource;
        public AudioClip[] clip;

        private int registeredParticlesCount;


        public void StartEmitting()
        {
            if (ps.isPlaying) return;

            ps.Play();
            ps.Emit(1);
        }


        private void Update()
        {
            if (!ps.isPlaying) return;

            int count = ps.particleCount;

            if (count < registeredParticlesCount)
            {
                asource.PlayOneShot(clip[Random.Range(0, clip.Length - 1)]);
            }

            registeredParticlesCount = count;
        }
    }
}
