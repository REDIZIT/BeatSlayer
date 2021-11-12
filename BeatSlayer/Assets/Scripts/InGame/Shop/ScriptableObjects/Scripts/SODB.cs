using InGame.Menu.Mods;
using InGame.Shop;
using System.Collections.Generic;
using UnityEngine;

namespace InGame.ScriptableObjects
{
    /// <summary>
    /// Database for all scriptable objects in game. To access it, just use instance.
    /// </summary>
    [CreateAssetMenu(menuName = "Shop/SODB")]
    public class SODB : ScriptableObject
    {
        public int lastUsedId = -1;

        [Header("Objects")]
        public List<SaberSO> sabers;
        public List<TailSO> tails;
        public List<LocationSO> locations;
        public List<ModSO> mods;

        private void OnValidate()
        {
            List<ShopItemSO> all = new List<ShopItemSO>();
            all.AddRange(sabers);
            all.AddRange(tails);
            all.AddRange(locations);



            foreach (var item in all)
            {
                if (item.purchaseId == -1)
                {
                    lastUsedId++;
                    item.purchaseId = lastUsedId;
                }
            }
        }
    }
}