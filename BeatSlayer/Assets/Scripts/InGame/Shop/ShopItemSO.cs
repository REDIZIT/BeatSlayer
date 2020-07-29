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

        [Tooltip("Purchase id in server database")]
        public int purchaseId = -1;

        [Tooltip("Purchase name in server database")]
        public string purchaseName;
    }
}
