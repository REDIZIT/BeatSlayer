using InGame.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Assets.SimpleLocalization;
using UnityEngine;
using UnityEngine.Advertisements;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneControllerUI : MonoBehaviour
{
    public AudioSource asource;
    
    public Animator window;
    public Text stateText, percentText;
    public Slider progressBar;

    public void Load()
    {
        StartCoroutine(IELoad()); 
    }

    IEnumerator IELoad()
    {
        SceneloadParameters parameters = LoadingData.loadparams;

        string sceneName = parameters.Type == SceneloadParameters.LoadType.Menu ? "Menu" : "Game";


        float loadStartTime = Time.realtimeSinceStartup;

        window.gameObject.SetActive(true);
        window.Play("StartLoading");


        AsyncOperation ao = SceneManager.LoadSceneAsync(sceneName);
        ao.allowSceneActivation = false;


        if(!File.Exists(Application.persistentDataPath + "/noads.txt"))
        {
            if (!Advertisement.isInitialized)
            {
                Advertisement.Initialize("3202418", false);
            }
            if (parameters.Type == SceneloadParameters.LoadType.Menu)
            {
                if (Advertisement.IsReady())
                {
                    Advertisement.Show("video");
                }
            }
        }



        float volumeDecrease = 0.05f;
        float volume = asource == null ? 0 : asource.volume;
        while (ao.progress < 0.9f || volume > 0.01f)
        {
            if (ao.progress >= 0.9f)
            {
                stateText.text = LocalizationManager.Localize("Done");
                percentText.text = "100%";
                progressBar.value = progressBar.maxValue;
            }
            else
            {
                stateText.text = LocalizationManager.Localize("Loading..");
                percentText.text = ao.progress + "%";
                progressBar.value = ao.progress;
            }
            
            if (asource != null)
            {
                asource.volume -= asource.volume * volumeDecrease;
                volume = asource.volume;
            }

            yield return new WaitForEndOfFrame();
        }

        float animEndIn = loadStartTime + (60f / 20f);
        float diff = loadStartTime - Time.realtimeSinceStartup;

        if (diff > 0) yield return new WaitForSeconds(diff);

        ao.allowSceneActivation = true;
    }
}