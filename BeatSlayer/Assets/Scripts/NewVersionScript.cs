using Assets.SimpleLocalization;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NewVersionScript : MonoBehaviour
{
    //public Text newVerLabel, newVerDescript;
    //public RectTransform content;

    public Text label;

    void Start()
    {
        //newVerLabel.text = Application.systemLanguage == SystemLanguage.Russian ? "Доступна новая версия, " + LCData.newVersion : "New version is available, " + LCData.newVersion;

        //newVerDescript.text = LCData.newVersionDescription;

        string lbl = LocalizationManager.Localize("NewVer");
        

        label.text = string.Format(lbl, Application.version, LCData.newVersion);
    }
    public void Update()
    {
        //content.GetComponent<RectTransform>().sizeDelta = new Vector2(content.GetComponent<RectTransform>().sizeDelta.x, newVerDescript.preferredHeight + 30);
    }
    public void GoToPlayMarket()
    {
        Application.OpenURL("https://play.google.com/store/apps/details?id=com.REDIZIT.BeatSlayer");
    }
}
