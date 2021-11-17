using GameNet;
using InGame.Shop;

namespace InGame.UI.Menu.Winter
{
    public abstract class WordReward
    {
        public abstract void Apply();
    }

    public class WordCoinsReward : WordReward
    {
        private readonly int coins;

        public WordCoinsReward(int coins)
        {
            this.coins = coins;
        }

        public override void Apply()
        {
            Payload.Account.AddCoins(coins);
        }

        public override string ToString()
        {
            return coins.ToString();
        }
    }

    public class WordPurchaseReward : WordReward
    {
        private readonly ShopService shop;
        private readonly int purchaseId = -1;

        public WordPurchaseReward(ShopService shop, int purchaseId)
        {
            this.shop = shop;
            this.purchaseId = purchaseId;
        }

        public override void Apply()
        {
            shop.TryBuy(purchaseId);
        }
    }
}