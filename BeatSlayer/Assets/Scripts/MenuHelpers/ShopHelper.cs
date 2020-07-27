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

public class ShopHelper : MonoBehaviour
{
    public MenuScript_v2 menuscript;
    public ShopColorSection colorSection;

    public GameObject tutorial;
    
    

    public Transform skillsScrollView, sabersView, effectsContent;
    public GameObject sabersGroup;
    Transform content;

    List<Skill> skills = new List<Skill>();
    public Sprite[] skillsSprites;

    List<Booster> boosters = new List<Booster>();
    public Sprite[] boostersSprites;

    public Transform colorselect;


    [Header("Views")]
    public GameObject[] viewsGameObjects;

    public SODB sodb;

    [Header("Item contents")]
    public Transform saberContent;

    private void Start()
    {
        content = skillsScrollView.GetChild(0).GetChild(0);
        skills = menuscript.PrefsManager.prefs.skills;
        boosters = menuscript.PrefsManager.prefs.boosters;
        UpdateSkillsView();

        RefreshSabersList();
        UpdateEffectsView();
    }




    // Cool stuff (relative) here


    public void ChangeView(int index)
    {
        foreach (var item in viewsGameObjects)
        {
            item.SetActive(item == viewsGameObjects[index]);
        }
    }


    public void RefreshSabersList()
    {
        HelperUI.FillContent<SaberShopItem, SaberSO>(saberContent, sodb.sabers, (item, data) =>
        {
            item.Refresh(data, menuscript.PrefsManager.prefs);
        });
    }
    


   


    // Not so cool there







    int selectedPage = 0;
    public void OnWindowChange(int id)
    {
        selectedPage = id;
        skillsScrollView.gameObject.SetActive(id != 2 && id != 3);
        sabersGroup.gameObject.SetActive(id == 2);
        effectsContent.parent.gameObject.SetActive(id == 3);

        if (id == 0) UpdateSkillsView();
        else if (id == 1) UpdateBoostersView();
        else if (id == 2) UpdateSabersView();
        else UpdateEffectsView();
    }



    void UpdateSkillsView()
    {
        foreach (Transform child in content) if (child.name != "Item") Destroy(child.gameObject);

        GameObject source = content.GetChild(0).gameObject;
        source.SetActive(true);

        int selectedId = menuscript.PrefsManager.prefs.skillSelected;
        if(selectedId != -1) CreateItem(source, skillsSprites[selectedId], skills[selectedId].name, skills[selectedId].description, skills[selectedId].cost, skills[selectedId].count, true);
        for (int i = 0; i < skills.Count; i++)
        {
            if (i == menuscript.PrefsManager.prefs.skillSelected) continue;
            CreateItem(source, skillsSprites[i], skills[i].name, skills[i].description, skills[i].cost, skills[i].count);
        }

        source.SetActive(false);
    }
    void UpdateBoostersView()
    {
        foreach (Transform child in content) if (child.name != "Item") Destroy(child.gameObject);

        GameObject source = content.GetChild(0).gameObject;
        source.SetActive(true);

        int selectedId = menuscript.PrefsManager.prefs.selectedBooster;
        if(selectedId != -1) CreateItem(source, boostersSprites[selectedId], boosters[selectedId].name, boosters[selectedId].description, boosters[selectedId].cost, boosters[selectedId].count, true);
        for (int i = 0; i < boosters.Count; i++)
        {
            if (i == menuscript.PrefsManager.prefs.selectedBooster) continue;
            CreateItem(source, boostersSprites[i], boosters[i].name, boosters[i].description, boosters[i].cost, boosters[i].count);
        }

        source.SetActive(false);
    }
    void UpdateSabersView()
    {
        HelperUI.RefreshContent<SaberShopItem, SaberSO>(saberContent, sodb.sabers, (item, data) =>
        {
            item.Refresh(data, menuscript.PrefsManager.prefs);
        }, 1);
    }
    public void UpdateEffectsView()
    {
        Transform eContent = effectsContent.GetChild(0);
        for (int i = 0; i < eContent.childCount; i++)
        {
            Transform item = eContent.GetChild(i).GetChild(0);
            item.GetChild(4).gameObject.SetActive(!menuscript.PrefsManager.prefs.boughtSaberEffects[i]);
            if (!menuscript.PrefsManager.prefs.boughtSaberEffects[i])
            {
                item.GetChild(4).GetChild(1).GetComponent<Text>().text = LocalizationManager.Localize("Cost") + ": " + menuscript.PrefsManager.prefs.saberEffectsCosts[i];
            }
            else
            {
                Color32 imageColor = i != menuscript.PrefsManager.prefs.selectedSaberEffect ? new Color32(12, 12, 12, 232) : new Color32(255, 128, 0, 232);
                item.GetComponent<Image>().color = imageColor;
                Color32 btnColor = i != menuscript.PrefsManager.prefs.selectedSaberEffect ? new Color32(255, 128, 0, 255) : new Color32(34, 34, 34, 255);
                item.GetChild(3).GetComponent<Image>().color = btnColor;
                Color32 textColor = i != menuscript.PrefsManager.prefs.selectedSaberEffect ? new Color32(34, 34, 34, 255) : new Color32(255, 255, 255, 255);
                string textStr = i != menuscript.PrefsManager.prefs.selectedSaberEffect ? LocalizationManager.Localize("Select") : LocalizationManager.Localize("Selected");
                item.GetChild(3).GetChild(0).GetComponent<Text>().color = textColor;
                item.GetChild(3).GetChild(0).GetComponent<Text>().text = textStr;
            }
        }
    }


    void CreateItem(GameObject source, Sprite sprite, string header, string description, int cost, int count, bool isSelected = false)
    {
        Transform item = Instantiate(source, source.transform.parent).transform;
        item.GetChild(0).GetComponent<Image>().sprite = sprite;
        item.GetChild(1).GetComponent<Text>().text = LocalizationManager.Localize(header);
        item.GetChild(2).GetComponent<Text>().text = LocalizationManager.Localize(description);
        item.GetChild(3).GetComponent<Button>().interactable = Payload.CurrentAccount == null ? false : Payload.CurrentAccount.Coins >= cost;
        string youhave = LocalizationManager.Localize("YouHave");
        string costStr = LocalizationManager.Localize("Cost");
        item.GetChild(4).GetComponent<Text>().text = youhave + ": " + count + "\n" + costStr + ": " + cost;

        if (isSelected)
        {
            item.GetComponent<Image>().color = new Color32(255, 128, 0, 255);
            item.GetChild(5).GetComponent<Image>().color = new Color32(34, 34, 34, 255);
            item.GetChild(5).GetChild(0).GetComponent<Text>().color = Color.white;
            item.GetChild(5).GetChild(0).GetComponent<Text>().text = LocalizationManager.Localize("Deselect");
        }
        else
        {
            item.GetChild(5).GetChild(0).GetComponent<Text>().text = LocalizationManager.Localize("Select");
        }
    }

    public void BuyBtnClick(Text text)
    {
        if (Payload.CurrentAccount == null) return;

        if(text.text == LocalizationManager.Localize("Time travel"))
        {
            skills[0].count++;
            Payload.CurrentAccount.Coins -= skills[0].cost;
            NetCore.ServerActions.Shop.SendCoins(Payload.CurrentAccount.Nick, -skills[0].cost);
        }
        else if (text.text == LocalizationManager.Localize("Explosion"))
        {
            skills[1].count++;
            Payload.CurrentAccount.Coins -= skills[1].cost;
            NetCore.ServerActions.Shop.SendCoins(Payload.CurrentAccount.Nick, -skills[1].cost);
        }
        else if (text.text == LocalizationManager.Localize("Coins booster (x2)"))
        {
            boosters[0].count++;
            Payload.CurrentAccount.Coins -= boosters[0].cost;
            NetCore.ServerActions.Shop.SendCoins(Payload.CurrentAccount.Nick, -boosters[0].cost);
        }
        else if (text.text == LocalizationManager.Localize("Coins decelerator (/2)"))
        {
            boosters[1].count++;
            Payload.CurrentAccount.Coins -= boosters[1].cost;
            NetCore.ServerActions.Shop.SendCoins(Payload.CurrentAccount.Nick, -boosters[1].cost);
        }

        //menuscript.coinsTexts[0].text = NetCorePayload.CurrentAccount.Coins.ToString();
        menuscript.RefreshCoinsTexts();
        menuscript.PrefsManager.Save();

        OnWindowChange(selectedPage);

        menuscript.CheckAchievement();
    }
    

    public void SelectBtnClick(Text text)
    {
        // Если нажатая кнопка имеет текст Deselect
        if (text.transform.parent.GetChild(5).GetChild(0).GetComponent<Text>().text == LocalizationManager.Localize("Deselect"))
        {
            if (text.text == LocalizationManager.Localize("Time travel") || text.text == LocalizationManager.Localize("Explosion")) menuscript.PrefsManager.prefs.skillSelected = -1;
            else if (text.text == LocalizationManager.Localize("Coins booster (x2)") || text.text == LocalizationManager.Localize("Coins decelerator (/2)"))menuscript.PrefsManager.prefs.selectedBooster = -1;
        }
        else
        {
            if (text.text == LocalizationManager.Localize("Time travel")) menuscript.PrefsManager.prefs.skillSelected = 0;
            else if (text.text == LocalizationManager.Localize("Explosion")) menuscript.PrefsManager.prefs.skillSelected = 1;
            else if (text.text == LocalizationManager.Localize("Coins booster (x2)")) menuscript.PrefsManager.prefs.selectedBooster = 0;
            else if (text.text == LocalizationManager.Localize("Coins decelerator (/2)")) menuscript.PrefsManager.prefs.selectedBooster = 1;
        }

        menuscript.PrefsManager.Save();
        OnWindowChange(selectedPage);
    }

    public void BuySaber(int id)
    {
        if (Payload.CurrentAccount == null) return;

        SaberSO saberSO = sodb.sabers.Find(c => c.id == id);

        if (!menuscript.PrefsManager.prefs.boughtSabers[id])
        {
            if (Payload.CurrentAccount.Coins >= saberSO.cost)
            {
                menuscript.PrefsManager.prefs.boughtSabers[id] = true;
                menuscript.PrefsManager.prefs.sabers[id].isBought = true;

                Payload.CurrentAccount.Coins -= saberSO.cost;
                NetCore.ServerActions.Shop.SendCoins(Payload.CurrentAccount.Nick, -saberSO.cost);

                menuscript.PrefsManager.Save();
                UpdateSabersView();

                menuscript.CheckAchievement();

                menuscript.RefreshCoinsTexts();
            }
        }
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
        UpdateSabersView();
    }

    public void BuySaberEffectClick(int id)
    {
        if (!menuscript.PrefsManager.prefs.boughtSaberEffects[id])
        {
            if (Payload.CurrentAccount.Coins >= menuscript.PrefsManager.prefs.saberEffectsCosts[id])
            {
                menuscript.PrefsManager.prefs.boughtSaberEffects[id] = true;
                Payload.CurrentAccount.Coins -= menuscript.PrefsManager.prefs.saberEffectsCosts[id];
                NetCore.ServerActions.Shop.SendCoins(Payload.CurrentAccount.Nick, -menuscript.PrefsManager.prefs.saberEffectsCosts[id]);
                menuscript.RefreshCoinsTexts();

                menuscript.PrefsManager.Save();
                UpdateEffectsView();

                menuscript.CheckAchievement();
            }
        }
    }

    public void SelectSaberEffectClick(int id)
    {
        menuscript.PrefsManager.prefs.selectedSaberEffect = id;
        menuscript.PrefsManager.Save();
        UpdateEffectsView();
    }









    public void OnShopOpen()
    {
        if (!menuscript.PrefsManager.prefs.shopTutorial)
        {
            menuscript.PrefsManager.prefs.shopTutorial = true;
            menuscript.PrefsManager.Save();
            tutorial.SetActive(true);
        }
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