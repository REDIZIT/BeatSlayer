using UnityEngine;
using Zenject;

namespace InGame.DI
{
    public class GameInstaller : MonoInstaller
    {
        [SerializeField] private SliceEffectSystem cubeSliceEffectPrefab;
        [SerializeField] private BeatCube cubePrefab;

        public override void InstallBindings()
        {
            Container.BindMemoryPool<SliceEffectSystem, SliceEffectSystem.Pool>()
                .WithInitialSize(10)
                .FromComponentInNewPrefab(cubeSliceEffectPrefab.gameObject);

            Container.BindMemoryPool<BeatCube, BeatCube.Pool>()
                .WithInitialSize(50)
                .FromComponentInNewPrefab(cubePrefab);
        }
    }
}
