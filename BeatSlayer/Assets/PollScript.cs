using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PollScript : MonoBehaviour
{
    //public PrefsManager manager;
    public AdvancedSaveManager manager;

    public int pollId;
    public string[] vars;

    private void Start()
    {
        CreatePoll("Что мне делать дальше?", new string[2] { "Toby Fox - Megalovania", "Handclap" });
    }

    float height = 60 + 15;

    public void CreatePoll(string label, string[] _vars)
    {
        vars = _vars;

        float width = 90 + 60;

        transform.GetChild(0).GetChild(0).GetComponent<Text>().text = label;
        width += transform.GetChild(0).GetChild(0).GetComponent<Text>().preferredWidth;

        Transform group = transform.GetChild(1);
        for (int i = 0; i < vars.Length; i++)
        {
            GameObject item = Instantiate(group.GetChild(0).gameObject, group);
            item.transform.GetChild(1).GetComponent<Text>().text = vars[i];
            item.name = vars[i];

            if(manager.prefs.lastPollId == pollId && manager.prefs.lastPollAnswer == vars[i])
            {
                item.GetComponent<Image>().color = new Color32(150, 150, 150, 150);
            }
        }
        group.GetChild(0).gameObject.SetActive(false);

        height += vars.Length * 60 + (vars.Length - 1) * 8;

        //transform.GetComponent<RectTransform>().sizeDelta = new Vector2(width, height);
    }

    public void OnItemClick(Transform item)
    {
        manager.prefs.lastPollId = pollId;
        manager.prefs.lastPollAnswer = item.name;
        manager.Save();

        for (int i = 1; i < item.parent.childCount; i++)
        {
            if (manager.prefs.lastPollAnswer == vars[i - 1])
            {
                item.GetComponent<Image>().color = new Color32(150, 150, 150, 150);
            }
            else
            {
                item.GetComponent<Image>().color = new Color32(64, 64, 64, 100);
            }
        }
    }

    public void OpenPollWindow()
    {
        StartCoroutine(openAnim());
    }
    public IEnumerator openAnim()
    {
        while(GetComponent<RectTransform>().sizeDelta.y < height - 1)
        {
            float diff = GetComponent<RectTransform>().sizeDelta.y - height;
            GetComponent<RectTransform>().sizeDelta += new Vector2(0, -diff / 8f);
            yield return new WaitForEndOfFrame();
        }
    }


    public void SendResponse(int pollId, string response)
    {

    }
}