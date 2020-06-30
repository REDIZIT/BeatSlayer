using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace InGame.Shop
{
    public class ShopManager : MonoBehaviour
    {
        public Transform sabersContent;

        public void UpdateSabersView()
        {
            bool isBought = false;

            for (int i = 0; i < sabersContent.childCount; i++)
            {
                Transform item = sabersContent.GetChild(i).GetChild(0);
                item.GetChild(4).gameObject.SetActive(!isBought);
                if (!isBought)
                {
                    item.GetChild(4).GetChild(1).GetComponent<Text>().text = "cost";
                }
                else
                {
                    Color32 imageColor = new Color32(12, 12, 12, 232);
                    item.GetComponent<Image>().color = imageColor;
                    Color32 btnColor = new Color32(255, 128, 0, 255);
                    item.GetChild(3).GetComponent<Image>().color = btnColor;
                    Color32 textColor = new Color32(34, 34, 34, 255);
                    string textStr = "textStr";
                    item.GetChild(3).GetChild(0).GetComponent<Text>().color = textColor;
                    item.GetChild(3).GetChild(0).GetComponent<Text>().text = textStr;
                }
            }

            //UpdateColors();
        }
    }
}