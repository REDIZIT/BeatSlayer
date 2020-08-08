using UnityEngine;
using UnityEngine.UI;

namespace InGame.Game.HP
{
    public class HPLocker : MonoBehaviour
    {
        public HPManager manager;

        public Image colorFilterOverlay;
        public GameObject gameOverWindow;

        [Header("Colors")]
        public Color maxColor;
        public Color gameOverLockerColor;

        [Header("Color filter")]
        public float colorFilterStartHP;
        public float colorFilterEndHP;

        private float HP => manager.HP;
        private bool IsAlive => manager.isAlive;


        private void Update()
        {
            gameOverWindow.SetActive(!IsAlive);
            if (!IsAlive)
            {
                colorFilterOverlay.color = gameOverLockerColor;
                return;
            }

            if (manager.IsNoFail) return;


            if (HP <= colorFilterStartHP && HP > colorFilterEndHP)
            {
                float diff = colorFilterStartHP - HP;

                diff /= colorFilterStartHP - colorFilterEndHP;

                colorFilterOverlay.color = diff * maxColor;
            }
            else if (HP <= colorFilterEndHP)
            {
                colorFilterOverlay.color = maxColor;
            }
            else
            {
                colorFilterOverlay.color = Color.clear;
            }
        }
    }
}
