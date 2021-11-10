using InGame.UI.Menu.Winter;
using Zenject;

public class ProjectInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        Container.Bind<WordEventManager>().AsSingle();
    }
}