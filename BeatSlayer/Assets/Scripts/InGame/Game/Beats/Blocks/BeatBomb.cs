using InGame.Game.Spawn;
using UnityEngine;
using Zenject;

namespace InGame.Game.Beats.Blocks
{
    public class BeatBomb : Beat, IBeat
    {
        public Transform Transform { get { return transform == null ? null : transform; } }


        public BeatCubeClass cls;
        public BeatCubeClass GetClass() { return cls; }

        /// <summary>
        /// Multiplier of cube calculated speed from 0 to 1
        /// </summary>
        public float SpeedMultiplier { get; set; }
        public float CurrentSpeed { get { return bm.CubeSpeedPerFrame * cls.speed; } }

        private BeatManager bm;
        private GameManager gm;

        private bool isDead;

        private Pool pool;
        private SliceEffectSystem.Pool effectPool;


        [Inject]
        private void Construct(Pool pool, SliceEffectSystem.Pool effectPool)
        {
            this.pool = pool;
            this.effectPool = effectPool;
        }

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
                Slice();
                return;
            }

            if (direction.normalized == Vector2.zero) return;

            Slice();
        }

        public void Destroy()
        {
            Slice();
        }

        void Slice()
        {
            if (isDead) return;
            isDead = true;

            gm.BeatCubeSliced(this);

            pool.Despawn(this);

            effectPool.Spawn().Play(transform.position, 0, BeatCubeClass.Type.Bomb);
        }


        void Movement()
        {
            transform.position += new Vector3(0, 0, -1) * CurrentSpeed * SpeedMultiplier;
            if (transform.position.z <= bm.maxDistance && !isDead)
            {
                gm.MissedBeatCube(this);

                pool.Despawn(this);
            }
        }

        public override void Reset()
        {
            
        }

        public class Pool : MonoMemoryPool<Beat>
        {
            protected override void Reinitialize(Beat item)
            {
                
            }
        }
    }
}
