using InGame.Game.Spawn;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

//
// https://youtu.be/pi0UXxCQg3w?t=286
//

public class CheatEngine : MonoBehaviour
{
    GameManager gm
    {
        get { return GetComponent<GameManager>(); }
    }
    public BeatManager bm;
    
    public AudioSource asource;
    public List<Beat> beats = new List<Beat>();

    public Image[] keys;

    [Header("Cheats")]
    [Range(0, 10)] public float pitch;
    public float time;
    public bool doWind, doSkipToEnd;
    public bool keyboardControl;

    [Header("Auto passing")]
    public bool autoSaber;
    public int makeMisses;

    private void Update()
    {
        if (!Application.isEditor) return;

        HandleHotkeys();
        
        foreach (Image key in keys)
        {
            key.color -= new Color(Time.deltaTime * 2, Time.deltaTime * 2, Time.deltaTime * 2,0);
        }

        if (asource.pitch != pitch)
        {
            asource.pitch = pitch;
        }

        if(beats.Count > 0)
        {
            if (autoSaber)
            {

                //List<IBeat> toPing = beats.Where(c => c != null && c.Transform != null && c.Transform.position.z - transform.position.z < 30).ToList();
                List<Beat> toPing = new List<Beat>();

                foreach (Beat beat in beats)
                {
                    if (beat == null) continue;
                    if (beat.Model == null) continue;

                    try
                    {
                        if (beat.Transform == null) continue;
                    }
                    catch
                    {
                        continue;
                    }

                    if (beat.Transform.position.z - transform.position.z < 24)
                    {
                        toPing.Add(beat);
                    }
                }

                foreach (Beat cube in toPing)
                {
                    if(cube.Model.type != BeatCubeClass.Type.Line)
                    {
                        if (makeMisses > 0) continue;
                    }

                    if (cube.Model.type == BeatCubeClass.Type.Bomb) continue;

                    cube.OnPoint(Vector2.zero, true);
                }
            }
        }

        if(doWind)
        {
            bm.SetBeats(bm.Beats.SkipWhile(c => c.time < time).ToList());
            asource.time = time;
            doWind = false;
        }

        if (doSkipToEnd)
        {
            doSkipToEnd = false;
            float endTime = asource.clip.length - 5;
            bm.SetBeats(bm.Beats?.SkipWhile(c => c.time < endTime).ToList());
            asource.time = endTime;
        }
    }

    private void HandleHotkeys()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (gm.paused) gm.Unpause();
            else gm.Pause();
        }
    }

    public void AddCube(Beat cube)
    {
        if (!Application.isEditor) return;

        beats.Add(cube);
    }
    public void RemoveCube(Beat beat)
    {
        if (!Application.isEditor) return;
        beats.Remove(beat);

        if (makeMisses > 0) makeMisses--;
    }
}
