using UnityEngine;

namespace InGame.Shop
{
    public class ShopItemSO : ScriptableObject
    {
        [Tooltip("Localization key for name")]
        public new string name;

        [Tooltip("Localization key for description")]
        public string description;

        [Tooltip("Coins to buy")]
        public int cost;
    }
}
