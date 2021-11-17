using InGame.Audio;
using InGame.ScriptableObjects;
using InGame.Shop;
using InGame.UI.Game.Winter;
using InGame.UI.Menu;
using InGame.UI.Menu.Winter;
using UnityEngine;
using Zenject;

public class ProjectInstaller : MonoInstaller
{
    [SerializeField] private SODB sodb;

    public override void InstallBindings()
    {
        Container.Bind<SODB>().FromInstance(sodb);
        Container.Bind<ShopService>().AsSingle();

        Container.Bind<WordEventManager>().AsSingle();
        Container.BindFactory<WordLetter, SpinnerWordLot, SpinnerWordLot.Factory>();

        Container.Bind<AudioService>().FromComponentInHierarchy().AsSingle();
    }
}