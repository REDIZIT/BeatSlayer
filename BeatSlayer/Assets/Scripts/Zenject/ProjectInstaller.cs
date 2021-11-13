using InGame.Audio;
using InGame.UI.Game.Winter;
using InGame.UI.Menu.Winter;
using Zenject;

public class ProjectInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        Container.Bind<WordEventManager>().AsSingle();
        Container.BindFactory<WordLetter, SpinnerWordLot, SpinnerWordLot.Factory>();

        Container.Bind<AudioService>().FromComponentInHierarchy().AsSingle();
    }
}