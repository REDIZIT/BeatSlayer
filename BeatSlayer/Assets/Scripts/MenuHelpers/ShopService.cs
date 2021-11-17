using GameNet;
using InGame.ScriptableObjects;
using System.Threading.Tasks;
using System.Linq;
using UnityEngine;
using BeatSlayerServer.Models.Database;

namespace InGame.Shop
{
    public class ShopService
    {
        private readonly SODB sodb;

        public ShopService(SODB sodb)
        {
            this.sodb = sodb;
        }

        public bool TryBuy(int purchaseId)
        {
            return Purchase(purchaseId).Wait(10 * 1000);
        }

        public async Task<bool> Purchase(int purchaseId)
        {
            if (Payload.Account == null) return false;

            ShopItemSO item = sodb.EnumerateAllItems().First(c => c.purchaseId == purchaseId);
            if (Payload.Account.Coins < item.cost) return false;

            // Check if already bought
            if (Payload.Account.Purchases.Any(c => c.ItemId == purchaseId)) return false;

            PurchaseModel purchase = await NetCore.ServerActions.Shop.TryBuyPurchase(Payload.Account.Nick, purchaseId);

            // Purchasing failed on server side
            if (purchase == null)
            {
                Debug.LogError("Purchase is null");
                return false;
            }

            // Adding bought purchase to local account class and decrease local balance
            Payload.Account.Purchases.Add(purchase);
            Payload.Account.Coins -= purchase.Cost;

            return true;
        }
    }
}