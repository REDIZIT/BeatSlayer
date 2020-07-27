using InGame.Game.Spawn;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
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
    //List<BeatCube> cubes = new List<BeatCube>();
    List<IBeat> beats = new List<IBeat>();

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
            //List<BeatCube> cubesToSlice = cubes.Where(c => c != null && Mathf.Abs(c.transform.position.z - transform.position.z) < 30).ToList();
            //if (Input.GetKeyDown(KeyCode.E))
            //{
            //    keys[2].color = new Color(1f, 0.6f, 0, 0.8f);
            //    BeatCube cube = cubesToSlice.Find(c => c.transform.position.x == 1.25f);
            //    if(cube != null) cube.OnPoint(Vector2.zero);
            //}
            //if (Input.GetKeyDown(KeyCode.W))
            //{
            //    keys[1].color = new Color(1f, 0.6f, 0, 0.8f);
            //    BeatCube cube = cubesToSlice.Find(c => c.transform.position.x == -1.25f);
            //    if (cube != null) cube.OnPoint(Vector2.zero);
            //}
            //if (Input.GetKeyDown(KeyCode.Q))
            //{
            //    keys[0].color = new Color(1f, 0.6f, 0, 0.8f);
            //    BeatCube cube = cubesToSlice.Find(c => c.transform.position.x == -3.5f);
            //    if (cube != null) cube.OnPoint(Vector2.zero);
            //}
            //if (Input.GetKeyDown(KeyCode.R))
            //{
            //    keys[3].color = new Color(1f, 0.6f, 0, 0.8f);
            //    BeatCube cube = cubesToSlice.Find(c => c.transform.position.x == 3.5f);
            //    if (cube != null) cube.OnPoint(Vector2.zero);
            //}


            if (autoSaber)
            {

                //List<IBeat> toPing = beats.Where(c => c != null && c.Transform != null && c.Transform.position.z - transform.position.z < 30).ToList();
                List<IBeat> toPing = new List<IBeat>();

                foreach (IBeat beat in beats)
                {
                    if (beat == null) continue;
                    if (beat.GetClass() == null) continue;

                    try
                    {
                        if (beat.Transform == null) continue;
                    }
                    catch(Exception err)
                    {
                        continue;
                    }

                    if (beat.Transform.position.z - transform.position.z < 24)
                    {
                        toPing.Add(beat);
                    }
                }

                foreach (IBeat cube in toPing)
                {
                    if(cube.GetClass().type != BeatCubeClass.Type.Line)
                    {
                        if (makeMisses > 0) continue;
                    }

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
            bm.SetBeats(bm.Beats.SkipWhile(c => c.time < endTime).ToList());
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

    public void AddCube(IBeat cube)
    {
        if (!Application.isEditor) return;

        //cubes.Add(cube);
        beats.Add(cube);
    }
    public void RemoveCube(IBeat beat)
    {
        if (!Application.isEditor) return;
        beats.Remove(beat);

        if (makeMisses > 0) makeMisses--;
    }
}
