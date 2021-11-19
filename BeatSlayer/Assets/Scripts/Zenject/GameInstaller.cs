using InGame.Game.Beats.Blocks;
using InGame.Game.Spawn;
using UnityEngine;
using Zenject;

namespace InGame.DI
{
    public class GameInstaller : MonoInstaller
    {
        [SerializeField] private GameManager gameManager;
        [SerializeField] private BeatManager beatManager;

        [Header("Prefabs")]
        [SerializeField] private SliceEffectSystem cubeSliceEffectPrefab;
        [SerializeField] private BeatCube cubePrefab;
        [SerializeField] private BeatBomb bombPrefab;
        [SerializeField] private BeatLine linePrefab;

        [SerializeField] private Transform poolContent;

        public override void InstallBindings()
        {
            InstallManagers();
            InstallBeats();
        }

        private void InstallManagers()
        {
            Container.Bind<GameManager>().FromInstance(gameManager).AsSingle();
            Container.Bind<BeatManager>().FromInstance(beatManager).AsSingle();
        }
        private void InstallBeats()
        {
            Container.BindMemoryPool<SliceEffectSystem, SliceEffectSystem.Pool>()
                .WithInitialSize(10)
                .FromComponentInNewPrefab(cubeSliceEffectPrefab.gameObject)
                .UnderTransform(poolContent);

            Container.BindMemoryPool<BeatCube, BeatCube.Pool>()
                .WithInitialSize(50)
                .FromComponentInNewPrefab(cubePrefab)
                .UnderTransform(poolContent);

            Container.BindMemoryPool<BeatBomb, BeatBomb.Pool>()
                .WithInitialSize(8)
                .FromComponentInNewPrefab(bombPrefab)
                .UnderTransform(poolContent);

            Container.BindMemoryPool<BeatLine, BeatLine.Pool>()
                .WithInitialSize(2)
                .FromComponentInNewPrefab(linePrefab)
                .UnderTransform(poolContent);
        }
    }

    public class BeatPool : MonoMemoryPool<BeatCubeClass, float, Beat>
    {
        protected override void OnCreated(Beat item)
        {
            base.OnCreated(item);
            item.Reset();
        }
        protected override void Reinitialize(BeatCubeClass p1, float p2, Beat item)
        {
            item.Reset();
            item.Setup(p1, p2);
        }
    }
}
