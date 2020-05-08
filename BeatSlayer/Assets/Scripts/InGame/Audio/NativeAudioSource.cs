using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NativeAudio
{
    public class NativeAudioSource : MonoBehaviour
    {
        public string filepath;
        public bool doPlay, doPause, doResume, doStop;
        
        void Start()
        {
            filepath = Application.persistentDataPath + "/data/audio/test.mp3";

            LoadFile(filepath, true);
        }

        private void Update()
        {
            if (doPlay)
            {
                doPlay = false;
                Play();                
            }
            if (doPause)
            {
                doPause = false;
                Pause();                
            }
            if (doResume)
            {
                doResume = false;
                Resume();                
            }
            if (doStop)
            {
                doStop = false;
                Stop();                
            }
        }


        public int id;


        public void LoadFile(string path, bool playOnLoad)
        {
            AndroidNativeAudio.makePool(1);
            AndroidNativeAudio.load(path, false, i =>
            {
                Debug.Log("File loaded with id " + i);
                id = i;

                if (playOnLoad) AndroidNativeAudio.play(id);
            });
        }

        public void Play()
        {
            AndroidNativeAudio.play(id);
        }

        public void Pause()
        {
            AndroidNativeAudio.pause(id);
        }

        public void Resume()
        {
            AndroidNativeAudio.resume(id);
        }

        public void Stop()
        {
            AndroidNativeAudio.stop(id);
        }
    }
}