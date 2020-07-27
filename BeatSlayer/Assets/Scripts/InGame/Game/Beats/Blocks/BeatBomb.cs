using InGame.Game.Spawn;
using UnityEngine;

namespace InGame.Game.Beats.Blocks
{
    public class BeatBomb : MonoBehaviour, IBeat
    {
        public Transform Transform { get { return transform == null ? null : transform; } }


        public BeatCubeClass cls;
        public BeatCubeClass GetClass() { return cls; }

        /// <summary>
        /// Multiplier of cube calculated speed from 0 to 1
        /// </summary>
        public float SpeedMultiplier { get; set; }
        public float CurrentSpeed { get { return bm.CubeSpeed * cls.speed; } }


        [SerializeField] private ParticleSystem bombParticleSystem;

        private BeatManager bm;
        private GameManager gm;

        private bool isDead;

        

        public void Setup(GameManager gm, BeatCubeClass cls, float cubesSpeed, BeatManager bm)
        {
            this.gm = gm;
            this.cls = cls;
            SpeedMultiplier = 1;
            this.bm = bm;

            if (cls.road == -1) cls.road = Random.Range(0, 3);

            float y = cls.level == 0 ? 0.8f : bm.secondHeight;
            Vector3 pos = new Vector3(bm.GetPositionByRoad(cls.road), y, 100);
            transform.position = pos;
        }

        void Update()
        {
            Movement();
        }


        public void OnPoint(Vector2 direction, bool destroy = false)
        {
            if (destroy)
            {
                Slice(0);
                return;
            }

            if (direction.normalized == Vector2.zero) return;

            Slice(0);
        }

        public void Destroy()
        {
            Slice(Random.Range(0, 360));
        }

        void Slice(float angle)
        {
            if (isDead) return;
            isDead = true;

            gm.BeatCubeSliced(this);

            OnSlice(angle);
        }


        void Movement()
        {
            transform.position += new Vector3(0, 0, -1) * CurrentSpeed * SpeedMultiplier;
            if (transform.position.z <= bm.maxDistance && !isDead)
            {
                gm.MissedBeatCube(this);

                Destroy(gameObject);
            }
        }

        void OnSlice(float angle)
        {
            bombParticleSystem.gameObject.SetActive(true);
            bombParticleSystem.transform.parent = null;
            bombParticleSystem.transform.eulerAngles = new Vector3(0, 0, angle);
            bombParticleSystem.Play();

            Destroy(gameObject);
        }
    }
}
