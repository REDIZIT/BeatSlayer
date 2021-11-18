using InGame.UI.Menu.Winter;
using UnityEngine;
using Zenject;

namespace InGame.DI
{
    public class MenuInstaller : MonoInstaller
    {
        [SerializeField] private ShopHelper shop;

        [SerializeField] private Transform wordContainerGroup;
        [SerializeField] private WordContainerUII wordContainerUII;

        public override void InstallBindings()
        {
            Container.Bind<ShopHelper>().FromInstance(shop);

            Container.BindFactory<WordContainerUII, WordContainerUII.Factory>()
                .FromComponentInNewPrefab(wordContainerUII)
                .UnderTransform(wordContainerGroup);
        }
    }
}