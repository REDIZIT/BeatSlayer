using InGame.UI.Menu.Winter;
using UnityEngine;
using Zenject;

public class MenuInstaller : MonoInstaller
{
    [SerializeField] private Transform wordContainerGroup;
    [SerializeField] private WordContainerUII wordContainerUII;

    public override void InstallBindings()
    {
        Container.BindFactory<WordContainerUII, WordContainerUII.Factory>()
            .FromComponentInNewPrefab(wordContainerUII)
            .UnderTransform(wordContainerGroup);
    }
}
