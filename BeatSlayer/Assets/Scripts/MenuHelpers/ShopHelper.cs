using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Assets.SimpleLocalization;
using GameNet;
using InGame.Helpers;
using InGame.Shop;
using InGame.ScriptableObjects;
using InGame.Menu.Shop;
using InGame.UI.Menu.Shop;
using System.Threading.Tasks;
using BeatSlayerServer.Models.Database;
using System.Linq;
using UnityEditor;
using Newtonsoft.Json;
using InGame.Shop.UIItems;

public class ShopHelper : MonoBehaviour
{
    public MenuScript_v2 menuscript;
    public ShopColorSection colorSection;

    public GameObject tutorial;
    
    

    public Transform skillsScrollView, sabersView;
    public GameObject sabersGroup;
    Transform content;

    List<Skill> skills = new List<Skill>();
    public Sprite[] skillsSprites;

    List<Booster> boosters = new List<Booster>();
    public Sprite[] boostersSprites;

    public Transform colorselect;


    [Header("Views")]
    public GameObject[] viewsGameObjects;
    public GameObject authLocker;

    public SODB sodb;

    [Header("Item contents")]
    public Transform saberContent;
    public Transform effectsContent, locationsContent;


    private void Awake()
    {
        NetCore.Configure(() =>
        {
            NetCore.OnLogIn += () =>
            {
                OnShopBtnClick();
                UpgradePurchases();
            };
        });
    }
    private void RefreshShop()
    {
        content = skillsScrollView.GetChild(0).GetChild(0);
        skills = menuscript.PrefsManager.prefs.skills;
        boosters = menuscript.PrefsManager.prefs.boosters;

        FillSabersView();
        FillEffectsView();
        FillLocationsView();
    }




    public void OnShopBtnClick()
    {
        authLocker.SetActive(Payload.Account == null);
        RefreshShop();
    }

    public void ChangeView(int index)
    {
        foreach (var item in viewsGameObjects)
        {
            item.SetActive(item == viewsGameObjects[index]);
        }
    }


    public void FillSabersView()
    {
        HelperUI.FillContent<SaberShopItem, SaberSO>(saberContent, sodb.sabers, (item, data) =>
        {
            item.Refresh(data, menuscript.PrefsManager.prefs);
        });
    }
    public void FillEffectsView()
    {
        HelperUI.FillContent<TailShopItem, TailSO>(effectsContent, sodb.tails, (item, data) =>
        {
            item.Refresh(data, menuscript.PrefsManager.prefs);
        });
    }
    public void FillLocationsView()
    {
        HelperUI.FillContent<LocationShopItem, LocationSO>(locationsContent, sodb.locations, (item, data) =>
        {
            item.Refresh(data, menuscript.PrefsManager.prefs);
        });
    }



    void RefreshSabersView()
    {
        HelperUI.RefreshContent<SaberShopItem, SaberSO>(saberContent, sodb.sabers, (item, data) =>
        {
            item.Refresh(data, menuscript.PrefsManager.prefs);
        });
    }
    public void RefreshEffectsView()
    {
        HelperUI.RefreshContent<TailShopItem, TailSO>(effectsContent, sodb.tails, (item, data) =>
        {
            item.Refresh(data, menuscript.PrefsManager.prefs);
        });
    }
    public void RefreshLocationsView()
    {
        HelperUI.RefreshContent<LocationShopItem, LocationSO>(locationsContent, sodb.locations, (item, data) =>
        {
            item.Refresh(data, menuscript.PrefsManager.prefs);
        });
    }



    public async Task BuySaber(int id)
    {
        if (Payload.Account == null) return;

        SaberSO saberSO = sodb.sabers.Find(c => c.id == id);
        if (Payload.Account.Coins < saberSO.cost) return;


        if (!await Purchase(saberSO.purchaseId)) return;

        // Updating UI
        RefreshSabersView();
        menuscript.RefreshCoinsTexts();
    }
   
    public async Task BuyTail(int id)
    {
        if (Payload.Account == null) return;

        TailSO tail = sodb.tails.Find(c => c.id == id);
        if (Payload.Account.Coins < tail.cost) return;


        if (!await Purchase(tail.purchaseId)) return;

        // Updating UI
        RefreshEffectsView();
        menuscript.RefreshCoinsTexts();
    }
    public async Task BuyLocation(LocationSO so)
    {
        if (Payload.Account == null) return;

        if (Payload.Account.Coins < so.cost) return;


        if (!await Purchase(so.purchaseId)) return;

        // Updating UI
        RefreshLocationsView();
        menuscript.RefreshCoinsTexts();
    }
    


    public void SelectSaber(int id, SaberHand hand)
    {
        //menuscript.prefsManager.prefs.selectedSaber = id;
        if (hand == SaberHand.Both)
        {
            menuscript.PrefsManager.prefs.selectedLeftSaberId = id;
            menuscript.PrefsManager.prefs.selectedRightSaberId = id;
        }
        else if (hand == SaberHand.Left)
        {
            menuscript.PrefsManager.prefs.selectedLeftSaberId = id;

            // If both -> left
            if (menuscript.PrefsManager.prefs.selectedRightSaberId == id)
            {
                menuscript.PrefsManager.prefs.selectedRightSaberId = 0;
            }
        }
        else
        {
            //menuscript.prefsManager.prefs.selectedLeftSaberId = -1;
            if (menuscript.PrefsManager.prefs.selectedLeftSaberId == id) menuscript.PrefsManager.prefs.selectedLeftSaberId = 0;
            menuscript.PrefsManager.prefs.selectedRightSaberId = id;
        }

        menuscript.PrefsManager.Save();
        RefreshSabersView();
    }
    public void SelectSaberEffectClick(int id)
    {
        menuscript.PrefsManager.prefs.selectedSaberEffect = id;
        menuscript.PrefsManager.Save();
        RefreshEffectsView();
    }
    public void SelectLocation(LocationSO so)
    {
        menuscript.PrefsManager.prefs.selectedMapId = so.id;
        menuscript.PrefsManager.Save();
        RefreshLocationsView();
    }




    public bool IsPurchased(ShopItemSO item)
    {
        if (item.cost == 0) return true;

        if (Payload.Account == null) return false;
        if (Payload.Account.Purchases == null) return false;

        return Payload.Account.Purchases.Any(c => c.Id == item.purchaseId);
    }
    public async void UpgradePurchases()
    {
        if (Payload.Account == null) return;
        if (Payload.Account.Purchases?.Count > 0) return;

        Debug.Log("Upgrading purchases..");

        List<PurchaseModel> upgradedList = await NetCore.ServerActions.Shop.UpgradePurchases(
            Payload.Account.Nick,
            menuscript.PrefsManager.prefs.boughtSabers, 
            menuscript.PrefsManager.prefs.boughtSaberEffects,
            new bool[] {
                true,
                menuscript.PrefsManager.prefs.mapUnlocked0,
                menuscript.PrefsManager.prefs.mapUnlocked1,
                menuscript.PrefsManager.prefs.mapUnlocked2,
                menuscript.PrefsManager.prefs.mapUnlocked3,
            });
        Debug.Log(JsonConvert.SerializeObject(upgradedList, Formatting.Indented));

        Payload.Account.Purchases = upgradedList;
    }


    int selectingColorForSaber = -1;
    public void OpenColorSelectWindow(int saber)
    {
        Debug.Log("OpenColorSelectWindow " + saber);

        selectingColorForSaber = saber;
        colorselect.gameObject.SetActive(true);
        Transform grid = colorselect.GetChild(0).GetChild(1).GetChild(0);

        Color saberColor;
        if (selectingColorForSaber == 0) saberColor = SSytem.instance.leftColor;
        else if (selectingColorForSaber == 1) saberColor = SSytem.instance.rightColor;
        else if (selectingColorForSaber == 2) saberColor = SSytem.instance.leftDirColor;
        else saberColor = SSytem.instance.rightDirColor;

        for (int i = 0; i < grid.childCount; i++)
        {
            Image img = grid.GetChild(i).GetComponent<Image>();

            img.transform.GetChild(0).gameObject.SetActive(saberColor == img.color);
            if (img.color == saberColor) break;
        }
    }
    public void SelectColorBtnClick(Image img)
    {
        if (selectingColorForSaber == 0) SSytem.instance.leftColor = img.color;
        else if (selectingColorForSaber == 1) SSytem.instance.rightColor = img.color;
        else if (selectingColorForSaber == 2) SSytem.instance.leftDirColor = img.color;
        else SSytem.instance.rightDirColor = img.color;
        menuscript.PrefsManager.Save();
        colorselect.gameObject.SetActive(false);
    }






    private async Task<bool> Purchase(int purchaseId)
    {
        // Check if already bought
        if (Payload.Account.Purchases.Any(c => c.Id == purchaseId)) return false;

        PurchaseModel purchase = await NetCore.ServerActions.Shop.TryBuyPurchase(Payload.Account.Nick, purchaseId);

        // Purchasing failed on server side
        if (purchase == null) return false;

        // Adding bought purchase to local account class and decrease local balance
        Payload.Account.Purchases.Add(purchase);
        Payload.Account.Coins -= purchase.Cost;

        return true;
    }
}


[Serializable]
public class Skill
{
    public string name, description;
    public int count;
    public int cost;
}

[Serializable]
public class Booster
{
    public string name, description;
    public int count;
    public int cost;
}

[Serializable]
public class Saber
{
    public int id;
    public bool isBought;
}