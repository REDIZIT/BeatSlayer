using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InGame.Mods
{
    public class ModsManager : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
    }


    public class Mod
    {
        public string name;
        public bool isActive;

        public virtual float GetCubeSpeed()
        {
            return 1;
        }
    }

    public class SlowDownMod : Mod
    {
        public SlowDownMod()
        {
            name = "SlowDown";
        }

        public override float GetCubeSpeed()
        {
            return 0.5f;
        }
    }
}