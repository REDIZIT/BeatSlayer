using InGame.Game.Spawn;
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
    
    public AudioSource asource;
    List<Bit> cubes = new List<Bit>();

    public Image[] keys;

    [Header("Cheats")]
    [Range(0, 10)] public float pitch;
    public float time;
    public bool doWind, doSkipToEnd;
    public bool keyboardControl;

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

        if(cubes.Count > 0)
        {
            List<Bit> cubesToSlice = cubes.Where(c => c != null && Mathf.Abs(c.transform.position.z - transform.position.z) < 30).ToList();
            if (Input.GetKeyDown(KeyCode.E))
            {
                keys[2].color = new Color(1f, 0.6f, 0, 0.8f);
                Bit cube = cubesToSlice.Find(c => c.transform.position.x == 1.25f);
                if(cube != null) cube.SendBitSliced(Vector2.zero);
            }
            if (Input.GetKeyDown(KeyCode.W))
            {
                keys[1].color = new Color(1f, 0.6f, 0, 0.8f);
                Bit cube = cubesToSlice.Find(c => c.transform.position.x == -1.25f);
                if (cube != null) cube.SendBitSliced(Vector2.zero);
            }
            if (Input.GetKeyDown(KeyCode.Q))
            {
                keys[0].color = new Color(1f, 0.6f, 0, 0.8f);
                Bit cube = cubesToSlice.Find(c => c.transform.position.x == -3.5f);
                if (cube != null) cube.SendBitSliced(Vector2.zero);
            }
            if (Input.GetKeyDown(KeyCode.R))
            {
                keys[3].color = new Color(1f, 0.6f, 0, 0.8f);
                Bit cube = cubesToSlice.Find(c => c.transform.position.x == 3.5f);
                if (cube != null) cube.SendBitSliced(Vector2.zero);
            }
        }

        if(doWind)
        {
            GetComponent<GameManager>().beats = GetComponent<GameManager>().beats.SkipWhile(c => c.time < time).ToList();
            asource.time = time;
            doWind = false;
        }

        if (doSkipToEnd)
        {
            doSkipToEnd = false;
            float endTime = asource.clip.length - 5;
            GetComponent<GameManager>().beats = GetComponent<GameManager>().beats.SkipWhile(c => c.time < endTime).ToList();
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

    public void AddCube(Bit cube)
    {
        if (!Application.isEditor) return;

        cubes.Add(cube);
    }
}
