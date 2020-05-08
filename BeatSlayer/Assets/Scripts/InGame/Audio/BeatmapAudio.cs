using System;
using System.Collections;
using System.Collections.Generic;
using DatabaseManagement;
using UnityEngine;
using UnityEngine.UI;

public class BeatmapAudio : MonoBehaviour
{
    public BeatmapUI ui;
    public AudioSource themeAsource;
    public AudioSource prelistenAsource;
    public DatabaseScript db;

    public Button prelistenBtn;

    public Image btnImage;
    public Sprite playSprite, pauseSprite;

    public Slider progressBar;

    
    
    public bool IsThemeEnabled // Is background music enabled
    {
        get { return SSytem.instance.GetBool("MenuMusic"); }
    } 
    
    public bool isPlaying; // Is prelisten asource playing


    private void Update()
    {
        if (isPlaying && !prelistenAsource.isPlaying)
        {
            OnPrelistenEnd();
            isPlaying = false;
        }

        if (prelistenAsource.isPlaying)
        {
            progressBar.value = prelistenAsource.time;
        }
    }


    public void ResetUI()
    {
        prelistenBtn.gameObject.SetActive(false);
        prelistenBtn.interactable = false;

        btnImage.sprite = playSprite;
        progressBar.value = 0;
    }

    public void OnOpen(string trackname)
    {
        db.HasPrelistenFile(trackname, b =>
        {
            if (b)
            {
                prelistenBtn.gameObject.SetActive(true);
                prelistenBtn.interactable = false;
                DownloadPrelisten(trackname);   
            }
        });
    }
    public void DownloadPrelisten(string trackname)
    {
        if (Application.internetReachability == NetworkReachability.NotReachable) return;
        
        db.LoadPrelistenFile(trackname, clip =>
        {
            if (clip != null)
            {
                prelistenBtn.gameObject.SetActive(true);
                prelistenBtn.interactable = true;
                btnImage.sprite = playSprite;
                prelistenAsource.clip = clip;
            }
            else
            {
                prelistenBtn.interactable = false;
                prelistenBtn.gameObject.SetActive(false);
            }
        });
    }

    public void OnPrelistenBtnClick()
    {
        if (prelistenAsource.isPlaying)
        {
            prelistenAsource.Pause();
            isPlaying = false;
            OnPrelistenEnd();
            btnImage.sprite = playSprite;
        }
        else
        {
            themeAsource.Pause();
            isPlaying = true;
            prelistenAsource.Play();
            btnImage.sprite = pauseSprite;
        }
        
        progressBar.maxValue = prelistenAsource.clip.length;
        progressBar.value = 0;
    }

    public void OnPrelistenEnd()
    {
        if(IsThemeEnabled) themeAsource.UnPause();
        btnImage.sprite = playSprite;
    }

    public void OnClose()
    {
        if(IsThemeEnabled) themeAsource.UnPause();
        prelistenAsource.Stop();
    }
}
