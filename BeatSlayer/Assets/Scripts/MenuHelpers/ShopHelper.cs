using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Assets.SimpleLocalization;

public class ShopHelper : MonoBehaviour
{
    public MenuScript_v2 menuscript;
    public GameObject tutorial;
    

    public Transform skillsScrollView, sabersView, effectsContent;
    public GameObject sabersGroup;
    Transform content;

    List<Skill> skills = new List<Skill>();
    public Sprite[] skillsSprites;

    List<Booster> boosters = new List<Booster>();
    public Sprite[] boostersSprites;

    public Image leftColorImg, rightColorImg, leftDirColorImg, rightArrowColorImg;
    public FlexibleColorPicker colorpicker;

    public Transform colorselect;

    private void Start()
    {
        content = skillsScrollView.GetChild(0).GetChild(0);
        skills = menuscript.prefsManager.prefs.skills;
        boosters = menuscript.prefsManager.prefs.boosters;

        UpdateSkillsView();
    }

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

        int selectedId = menuscript.prefsManager.prefs.skillSelected;
        if(selectedId != -1) CreateItem(source, skillsSprites[selectedId], skills[selectedId].name, skills[selectedId].description, skills[selectedId].cost, skills[selectedId].count, true);
        for (int i = 0; i < skills.Count; i++)
        {
            if (i == menuscript.prefsManager.prefs.skillSelected) continue;
            CreateItem(source, skillsSprites[i], skills[i].name, skills[i].description, skills[i].cost, skills[i].count);
        }

        source.SetActive(false);
    }
    void UpdateBoostersView()
    {
        foreach (Transform child in content) if (child.name != "Item") Destroy(child.gameObject);

        GameObject source = content.GetChild(0).gameObject;
        source.SetActive(true);

        int selectedId = menuscript.prefsManager.prefs.selectedBooster;
        if(selectedId != -1) CreateItem(source, boostersSprites[selectedId], boosters[selectedId].name, boosters[selectedId].description, boosters[selectedId].cost, boosters[selectedId].count, true);
        for (int i = 0; i < boosters.Count; i++)
        {
            if (i == menuscript.prefsManager.prefs.selectedBooster) continue;
            CreateItem(source, boostersSprites[i], boosters[i].name, boosters[i].description, boosters[i].cost, boosters[i].count);
        }

        source.SetActive(false);
    }
    void UpdateSabersView()
    {
        Transform sabersContent = sabersView.GetChild(0);
        for (int i = 0; i < sabersContent.childCount; i++)
        {
            Transform item = sabersContent.GetChild(i).GetChild(0);
            item.GetChild(4).gameObject.SetActive(!menuscript.prefsManager.prefs.boughtSabers[i]);
            if (!menuscript.prefsManager.prefs.boughtSabers[i])
            {
                item.GetChild(4).GetChild(1).GetComponent<Text>().text = LocalizationManager.Localize("Cost") + ": " + menuscript.prefsManager.prefs.sabersCosts[i];
            }
            else
            {
                Color32 imageColor = i != menuscript.prefsManager.prefs.selectedSaber ? new Color32(12, 12, 12, 232) : new Color32(255, 128, 0, 232);
                item.GetComponent<Image>().color = imageColor;
                Color32 btnColor = i != menuscript.prefsManager.prefs.selectedSaber ? new Color32(255, 128, 0, 255) : new Color32(34, 34, 34, 255);
                item.GetChild(3).GetComponent<Image>().color = btnColor;
                Color32 textColor = i != menuscript.prefsManager.prefs.selectedSaber ? new Color32(34, 34, 34, 255) : new Color32(255,255,255,255);
                string textStr = i != menuscript.prefsManager.prefs.selectedSaber ? LocalizationManager.Localize("Select") : LocalizationManager.Localize("Selected");
                item.GetChild(3).GetChild(0).GetComponent<Text>().color = textColor;
                item.GetChild(3).GetChild(0).GetComponent<Text>().text = textStr;
            }
        }

        UpdateColors();
    }
    void UpdateEffectsView()
    {
        Transform eContent = effectsContent.GetChild(0);
        for (int i = 0; i < eContent.childCount; i++)
        {
            Transform item = eContent.GetChild(i).GetChild(0);
            item.GetChild(4).gameObject.SetActive(!menuscript.prefsManager.prefs.boughtSaberEffects[i]);
            if (!menuscript.prefsManager.prefs.boughtSaberEffects[i])
            {
                item.GetChild(4).GetChild(1).GetComponent<Text>().text = LocalizationManager.Localize("Cost") + ": " + menuscript.prefsManager.prefs.saberEffectsCosts[i];
            }
            else
            {
                Color32 imageColor = i != menuscript.prefsManager.prefs.selectedSaberEffect ? new Color32(12, 12, 12, 232) : new Color32(255, 128, 0, 232);
                item.GetComponent<Image>().color = imageColor;
                Color32 btnColor = i != menuscript.prefsManager.prefs.selectedSaberEffect ? new Color32(255, 128, 0, 255) : new Color32(34, 34, 34, 255);
                item.GetChild(3).GetComponent<Image>().color = btnColor;
                Color32 textColor = i != menuscript.prefsManager.prefs.selectedSaberEffect ? new Color32(34, 34, 34, 255) : new Color32(255, 255, 255, 255);
                string textStr = i != menuscript.prefsManager.prefs.selectedSaberEffect ? LocalizationManager.Localize("Select") : LocalizationManager.Localize("Selected");
                item.GetChild(3).GetChild(0).GetComponent<Text>().color = textColor;
                item.GetChild(3).GetChild(0).GetComponent<Text>().text = textStr;
            }
        }

        UpdateColors();
    }


    void CreateItem(GameObject source, Sprite sprite, string header, string description, int cost, int count, bool isSelected = false)
    {
        Transform item = Instantiate(source, source.transform.parent).transform;
        item.GetChild(0).GetComponent<Image>().sprite = sprite;
        item.GetChild(1).GetComponent<Text>().text = LocalizationManager.Localize(header);
        item.GetChild(2).GetComponent<Text>().text = LocalizationManager.Localize(description);
        item.GetChild(3).GetComponent<Button>().interactable = menuscript.prefsManager.prefs.coins >= cost;
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
        if(text.text == LocalizationManager.Localize("Time travel"))
        {
            skills[0].count++;
            menuscript.prefsManager.prefs.coins -= skills[0].cost;
        }
        else if (text.text == LocalizationManager.Localize("Explosion"))
        {
            skills[1].count++;
            menuscript.prefsManager.prefs.coins -= skills[1].cost;
        }
        else if (text.text == LocalizationManager.Localize("Coins booster (x2)"))
        {
            boosters[0].count++;
            menuscript.prefsManager.prefs.coins -= boosters[0].cost;
        }
        else if (text.text == LocalizationManager.Localize("Coins decelerator (/2)"))
        {
            boosters[1].count++;
            menuscript.prefsManager.prefs.coins -= boosters[1].cost;
        }

        menuscript.coinsTexts[0].text = menuscript.prefsManager.prefs.coins.ToString();
        menuscript.prefsManager.Save();

        OnWindowChange(selectedPage);

        menuscript.CheckAchievement();
    }

    public void SelectBtnClick(Text text)
    {
        // Если нажатая кнопка имеет текст Deselect
        if (text.transform.parent.GetChild(5).GetChild(0).GetComponent<Text>().text == LocalizationManager.Localize("Deselect"))
        {
            if (text.text == LocalizationManager.Localize("Time travel") || text.text == LocalizationManager.Localize("Explosion")) menuscript.prefsManager.prefs.skillSelected = -1;
            else if (text.text == LocalizationManager.Localize("Coins booster (x2)") || text.text == LocalizationManager.Localize("Coins decelerator (/2)"))menuscript.prefsManager.prefs.selectedBooster = -1;
        }
        else
        {
            if (text.text == LocalizationManager.Localize("Time travel")) menuscript.prefsManager.prefs.skillSelected = 0;
            else if (text.text == LocalizationManager.Localize("Explosion")) menuscript.prefsManager.prefs.skillSelected = 1;
            else if (text.text == LocalizationManager.Localize("Coins booster (x2)")) menuscript.prefsManager.prefs.selectedBooster = 0;
            else if (text.text == LocalizationManager.Localize("Coins decelerator (/2)")) menuscript.prefsManager.prefs.selectedBooster = 1;
        }

        menuscript.prefsManager.Save();
        OnWindowChange(selectedPage);
    }

    public void ButSaberClick(int id)
    {
        if (!menuscript.prefsManager.prefs.boughtSabers[id])
        {
            if (menuscript.prefsManager.prefs.coins >= menuscript.prefsManager.prefs.sabersCosts[id])
            {
                menuscript.prefsManager.prefs.boughtSabers[id] = true;
                menuscript.prefsManager.prefs.coins -= menuscript.prefsManager.prefs.sabersCosts[id];

                menuscript.prefsManager.Save();
                UpdateSabersView();

                menuscript.CheckAchievement();

                menuscript.coinsTexts[0].text = menuscript.prefsManager.prefs.coins.ToString();
            }
        }
    }

    public void SelectSaberClick(int id)
    {
        menuscript.prefsManager.prefs.selectedSaber = id;
        menuscript.prefsManager.Save();
        UpdateSabersView();
    }

    public void BuySaberEffectClick(int id)
    {
        if (!menuscript.prefsManager.prefs.boughtSaberEffects[id])
        {
            if (menuscript.prefsManager.prefs.coins >= menuscript.prefsManager.prefs.saberEffectsCosts[id])
            {
                menuscript.prefsManager.prefs.boughtSaberEffects[id] = true;
                menuscript.prefsManager.prefs.coins -= menuscript.prefsManager.prefs.saberEffectsCosts[id];

                menuscript.prefsManager.Save();
                UpdateEffectsView();

                menuscript.CheckAchievement();

                menuscript.coinsTexts[0].text = menuscript.prefsManager.prefs.coins.ToString();
            }
        }
    }

    public void SelectSaberEffectClick(int id)
    {
        menuscript.prefsManager.prefs.selectedSaberEffect = id;
        menuscript.prefsManager.Save();
        UpdateEffectsView();
    }









    public void OnShopOpen()
    {
        if (!menuscript.prefsManager.prefs.shopTutorial)
        {
            menuscript.prefsManager.prefs.shopTutorial = true;
            menuscript.prefsManager.Save();
            tutorial.SetActive(true);
        }
    }



    void UpdateColors()
    {
        leftColorImg.color = SSytem.instance.leftColor;
        rightColorImg.color = SSytem.instance.rightColor;
        leftDirColorImg.color = SSytem.instance.leftDirColor;
        rightArrowColorImg.color = SSytem.instance.rightDirColor;
    }
    int colorpickingForId;
    public void OpenColorPicker(int colorId)
    {
        colorpicker.transform.parent.gameObject.SetActive(true);
        colorpicker.color = colorId == -1 ? leftColorImg.color : rightColorImg.color;
        colorpicker.startingColor = colorpicker.color;
        colorpickingForId = colorId;
        colorpicker.callback = OnColorpicked;
    }
    public void OnColorpicked(Color clr)
    {
        if (colorpickingForId == -1) SSytem.instance.leftColor = clr;
        if (colorpickingForId == 1) SSytem.instance.rightColor = clr;
        UpdateColors();
        //menuscript.prefsManager.Save();
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
        menuscript.prefsManager.Save();
        colorselect.gameObject.SetActive(false);
        UpdateColors();
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