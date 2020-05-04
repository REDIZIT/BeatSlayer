using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InGame.UI
{
    public class LoadingCircle : MonoBehaviour
    {
        public Animator animator;  
        
        public void Play()
        {
            animator.gameObject.SetActive(true);
        }

        public void Stop()
        {
            animator.gameObject.SetActive(false);
        }
    }
}