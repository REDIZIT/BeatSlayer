using InGame.Game.Beats.Blocks;
using UnityEngine;
using Zenject;

namespace InGame.DI
{
    public class GameInstaller : MonoInstaller
    {
        [SerializeField] private SliceEffectSystem cubeSliceEffectPrefab;
        [SerializeField] private BeatCube cubePrefab;
        [SerializeField] private BeatBomb bombPrefab;
        [SerializeField] private BeatLine linePrefab;

        [SerializeField] private Transform poolContent;

        public override void InstallBindings()
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
}
