using BeatSlayerServer.Models.Database;
using GameNet;
using InGame.Helpers;
using InGame.ScriptableObjects;
using InGame.Shop;
using InGame.Shop.UIItems;
using InGame.UI.Menu.Shop;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using Debug = UnityEngine.Debug;

public class ShopHelper : MonoBehaviour
{
    public MenuScript_v2 menuscript;
    public ShopColorSection colorSection;

    public GameObject tutorial;
    
    

    public Transform skillsScrollView, sabersView;
    public GameObject sabersGroup;

    public Sprite[] skillsSprites;

    public Sprite[] boostersSprites;

    public Transform colorselect;


    [Header("Views")]
    public GameObject[] viewsGameObjects;
    public GameObject authLocker;

    public SODB sodb;

    [Header("Item contents")]
    public Transform saberContent;
    public Transform effectsContent, locationsContent;

    private ShopService shop;


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

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            Debug.Log("Debug key pressed (L). Invoke shop test");

            shop.TryBuy(16);
        }
    }

    [Inject]
    private void Construct(ShopService shop)
    {
        this.shop = shop;
    }

    private void RefreshShop()
    {
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
            item.Refresh(data, AdvancedSaveManager.prefs);
        });
    }
    public void FillEffectsView()
    {
        HelperUI.FillContent<TailShopItem, TailSO>(effectsContent, sodb.tails, (item, data) =>
        {
            item.Refresh(data, AdvancedSaveManager.prefs);
        });
    }
    public void FillLocationsView()
    {
        HelperUI.FillContent<LocationShopItem, LocationSO>(locationsContent, sodb.locations, (item, data) =>
        {
            item.Refresh(data, AdvancedSaveManager.prefs);
        });
    }



    void RefreshSabersView()
    {
        HelperUI.RefreshContent<SaberShopItem, SaberSO>(saberContent, sodb.sabers, (item, data) =>
        {
            item.Refresh(data, AdvancedSaveManager.prefs);
        });
    }
    public void RefreshEffectsView()
    {
        HelperUI.RefreshContent<TailShopItem, TailSO>(effectsContent, sodb.tails, (item, data) =>
        {
            item.Refresh(data, AdvancedSaveManager.prefs);
        });
    }
    public void RefreshLocationsView()
    {
        HelperUI.RefreshContent<LocationShopItem, LocationSO>(locationsContent, sodb.locations, (item, data) =>
        {
            item.Refresh(data, AdvancedSaveManager.prefs);
        });
    }



    public async Task BuySaber(int id)
    {
        if (Payload.Account == null) return;

        SaberSO saberSO = sodb.sabers.Find(c => c.id == id);
        if (Payload.Account.Coins < saberSO.cost) return;



        // Updating UI
        RefreshSabersView();

        await shop.Purchase(saberSO.purchaseId);
    }
   
    public async Task BuyTail(int id)
    {
        if (Payload.Account == null) return;

        TailSO tail = sodb.tails.Find(c => c.id == id);
        if (Payload.Account.Coins < tail.cost) return;

        // Updating UI
        RefreshEffectsView();

        await shop.Purchase(tail.purchaseId);
    }
    public async Task BuyLocation(LocationSO so)
    {
        if (Payload.Account == null) return;

        if (Payload.Account.Coins < so.cost) return;

        // Updating UI
        RefreshLocationsView();

        await shop.Purchase(so.purchaseId);
    }
    


    public void SelectSaber(int id, SaberHand hand)
    {
        if (hand == SaberHand.Both)
        {
            AdvancedSaveManager.prefs.selectedLeftSaberId = id;
            AdvancedSaveManager.prefs.selectedRightSaberId = id;
        }
        else if (hand == SaberHand.Left)
        {
            AdvancedSaveManager.prefs.selectedLeftSaberId = id;

            // If both -> left
            if (AdvancedSaveManager.prefs.selectedRightSaberId == id)
            {
                AdvancedSaveManager.prefs.selectedRightSaberId = 0;
            }
        }
        else
        {
            if (AdvancedSaveManager.prefs.selectedLeftSaberId == id) AdvancedSaveManager.prefs.selectedLeftSaberId = 0;
            AdvancedSaveManager.prefs.selectedRightSaberId = id;
        }

        AdvancedSaveManager.Save();
        RefreshSabersView();
    }
    public void SelectSaberEffectClick(int id)
    {
        AdvancedSaveManager.prefs.selectedSaberEffect = id;
        AdvancedSaveManager.Save();
        RefreshEffectsView();
    }
    public void SelectLocation(LocationSO so)
    {
        AdvancedSaveManager.prefs.selectedMapId = so.id;
        AdvancedSaveManager.Save();
        RefreshLocationsView();
    }




    public bool IsPurchased(ShopItemSO item)
    {
        if (item.cost == 0) return true;

        if (Payload.Account == null) return false;
        if (Payload.Account.Purchases == null) return false;

        return Payload.Account.Purchases.Any(c => c.ItemId == item.purchaseId);
    }
    public async void UpgradePurchases()
    {
        if (Payload.Account == null) return;
        if (Payload.Account.Purchases?.Count > 0) return;

        Debug.Log("Upgrading purchases..");

        List<PurchaseModel> upgradedList = await NetCore.ServerActions.Shop.UpgradePurchases(
            Payload.Account.Nick,
            AdvancedSaveManager.prefs.boughtSabers,
            AdvancedSaveManager.prefs.boughtSaberEffects,
            new bool[] {
                true,
                AdvancedSaveManager.prefs.mapUnlocked0,
                AdvancedSaveManager.prefs.mapUnlocked1,
                AdvancedSaveManager.prefs.mapUnlocked2,
                AdvancedSaveManager.prefs.mapUnlocked3,
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
        if (selectingColorForSaber == 0) saberColor = SSytem.leftColor;
        else if (selectingColorForSaber == 1) saberColor = SSytem.rightColor;
        else if (selectingColorForSaber == 2) saberColor = SSytem.leftDirColor;
        else saberColor = SSytem.rightDirColor;

        for (int i = 0; i < grid.childCount; i++)
        {
            Image img = grid.GetChild(i).GetComponent<Image>();

            img.transform.GetChild(0).gameObject.SetActive(saberColor == img.color);
            if (img.color == saberColor) break;
        }
    }
    public void SelectColorBtnClick(Image img)
    {
        if (selectingColorForSaber == 0) SSytem.leftColor = img.color;
        else if (selectingColorForSaber == 1) SSytem.rightColor = img.color;
        else if (selectingColorForSaber == 2) SSytem.leftDirColor = img.color;
        else SSytem.rightDirColor = img.color;
        AdvancedSaveManager.Save();
        colorselect.gameObject.SetActive(false);
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