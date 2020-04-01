using InGame.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneControllerUI : MonoBehaviour
{
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

        AsyncOperation ao = SceneManager.LoadSceneAsync(sceneName);
        ao.allowSceneActivation = false;

        window.gameObject.SetActive(true);
        window.Play("StartLoading");

        float loadStartTime = Time.realtimeSinceStartup;



        while (ao.progress < 0.9f)
        {
            stateText.text = "Loading..";
            percentText.text = ao.progress + "%";
            progressBar.value = ao.progress;

            yield return new WaitForEndOfFrame();
        }




        stateText.text = "Loaded";
        percentText.text = "100%";
        progressBar.value = progressBar.maxValue;

        yield return new WaitForEndOfFrame();

        float animEndIn = loadStartTime + (60f / 20f);
        float diff = loadStartTime - Time.realtimeSinceStartup;

        if (diff > 0) yield return new WaitForSeconds(diff);

        ao.allowSceneActivation = true;
    }
}