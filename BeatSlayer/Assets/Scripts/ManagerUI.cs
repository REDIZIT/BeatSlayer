using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pixelplacement;
using Pixelplacement.TweenSystem;
using TMPro;
using UnityEngine.UI;
using Coffee.UIExtensions;
using UnityEngine.UI.Extensions;
using Imphenzia;

public class ManagerUI : Singleton<ManagerUI>
{
    [SerializeField] private RectTransform mainUI;
    [SerializeField] private RawImage bckgImgDeadScreen;

    [Space(25)]
    [SerializeField] private TextMeshProUGUI titleTxt;
    [SerializeField] private Transform[] titleBtns;

    [SerializeField] private AnimationCurve gameScreenDownAnim;

    


    [Space(25)]
    [SerializeField] private GradientSkyCamera skyBckg;
 



  
    [SerializeField] private State gameScreen;
    [SerializeField] private State homeScreen;
    [SerializeField] private State deadScreen;

    TweenBase killingVolume;
    TweenBase reviveHandle;

    public GradientSkyCamera SkyBckg { get => skyBckg; set => skyBckg = value; }

    private void Start()
    {

        AppStartAnims();
        
    }

    

    

    

    public void StartGame()
    {
        gameScreen.ChangeState(gameScreen.gameObject);


    }

    public void AppStartAnims()
    {
        foreach (Transform tf in titleBtns)
        {
            tf.localScale = Vector3.zero;

        }

        Tween.AnchoredPosition(titleTxt.GetComponent<RectTransform>(),
           new Vector2(0, mainUI.sizeDelta.y)
           , Vector2.zero, 0.25f, 0, Tween.EaseLinear,
           completeCallback: () =>
           {
               int num = 1;
               foreach (Transform tf in titleBtns)
               {
                   Tween.LocalScale(tf, Vector3.one, 0.4f, num++ / 8.0f, Tween.EaseInOut);
               }
           });

    }

    public void GotoGameScreen()
    {
        foreach (Transform tf in titleBtns)
        {
            Tween.LocalScale(tf, Vector3.zero, 0.25f, 0, Tween.EaseInOut);
        }

        Tween.AnchoredPosition(titleTxt.GetComponent<RectTransform>(), Vector2.zero,
           new Vector2(0, mainUI.sizeDelta.y)
           , 0.25f, 0, Tween.EaseInOut,
           completeCallback: () =>
           {


               StartGame();
           });


    }

    public void GotoHomeScreen()
    {
        foreach (Transform tf in titleBtns)
        {
            tf.localScale = Vector3.one;

        }
        titleTxt.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        homeScreen.ChangeState(homeScreen.gameObject);

        Tween.AnchoredPosition(homeScreen.GetComponent<RectTransform>(),
           new Vector2(0, mainUI.sizeDelta.y), Vector2.zero
           , 0.25f, 0, Tween.EaseInOut);
    }

}
