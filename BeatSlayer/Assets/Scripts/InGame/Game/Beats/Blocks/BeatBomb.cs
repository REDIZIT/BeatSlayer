using InGame.DI;
using UnityEngine;
using Zenject;

namespace InGame.Game.Beats.Blocks
{
    public class BeatBomb : Beat
    {
        private bool isDead;

        private Pool pool;
        private SliceEffectSystem.Pool effectPool;


        [Inject]
        private void Construct(Pool pool, SliceEffectSystem.Pool effectPool)
        {
            this.pool = pool;
            this.effectPool = effectPool;
        }

        public override void Setup(BeatCubeClass cls, float cubesSpeed)
        {
            base.Setup(cls, cubesSpeed);

            if (cls.road == -1) cls.road = Random.Range(0, 3);

            float y = cls.level == 0 ? 0.8f : bm.secondHeight;
            Vector3 pos = new Vector3(bm.GetPositionByRoad(cls.road), y, 100);
            transform.position = pos;
        }

        void Update()
        {
            Movement();
        }


        public override void OnPoint(Vector2 direction, bool destroy = false)
        {
            if (destroy)
            {
                Slice();
                return;
            }

            if (direction.normalized == Vector2.zero) return;

            Slice();
        }

        public override void Destroy()
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

        public class Pool : BeatPool
        {
        }
    }
}
