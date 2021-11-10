using InGame.UI.Menu.Winter;
using UnityEngine;
using Zenject;

public class ProjectInstaller : MonoInstaller
{
    [SerializeField] private Transform wordContainerGroup;
    [SerializeField] private WordContainerUII wordContainerUII;

    public override void InstallBindings()
    {
        Container.Bind<WordEvent>().FromNew().AsSingle();
        Container.BindFactory<Word, WordContainerUII, WordContainerUII.Factory>()
            .FromComponentInNewPrefab(wordContainerUII)
            .UnderTransform(wordContainerGroup);
    }
}