using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Testing
{
    public class TestAngle : MonoBehaviour
    {
        public Vector3 pos;
        public Vector3 startPos;
        public Vector3 direction;

        public BeatCubeClass.SubType subt;


        [Header("Logs")] 
        public bool willSlice;
        public float i, targetDeg, degrees, anglediff;

        private void Update()
        {
            direction = pos - startPos;
            direction = new Vector3(-direction.x, direction.y);

            float angle = Mathf.Atan2(direction.x, direction.y);
            degrees = Mathf.Rad2Deg * angle;
            if (degrees < 0) degrees = 360 + degrees;

            
            i = Mathf.Repeat((int)subt + 4, 8);
            targetDeg = i * 45;

            anglediff = (degrees - targetDeg + 180 + 360) % 360 - 180;
            

            if (anglediff <= 45 && anglediff >= -45)
            {
                willSlice = true;
            }
            else
            {
                willSlice = false;
            }
        }
    }
}