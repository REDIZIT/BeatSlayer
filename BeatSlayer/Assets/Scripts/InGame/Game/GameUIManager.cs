using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using UnityEngine.Rendering.PostProcessing;

public class GameUIManager : MonoBehaviour
{
    //public PostProcessVolume postProcessVolume;
    //DepthOfField depthOfField;
    public float focusTarget;

    public float focusingTime;
    float _focusingTime;

    public GameObject pauseOverlay;


    private void Start()
    {
        //depthOfField = postProcessVolume.profile.GetSetting<DepthOfField>();
        _focusingTime = focusingTime;
    }
    private void Update()
    {
        //Animate();
    }

    void Animate()
    {
        float target = focusTarget;
        //float current = depthOfField.focusDistance.value;

        //depthOfField.focusDistance.value += (target - current) / _focusingTime;
    }

    public void OnPause()
    {
        //pauseOverlay.SetActive(true);
        focusTarget = 0;
        _focusingTime = focusingTime;
    }
    public void OnResume()
    {
        //pauseOverlay.SetActive(true);
        focusTarget = 5;
        _focusingTime = focusingTime * 4f;
    }
    public void OnFinish()
    {
        focusTarget = 5;
        _focusingTime = focusingTime / 2f;
    }
}