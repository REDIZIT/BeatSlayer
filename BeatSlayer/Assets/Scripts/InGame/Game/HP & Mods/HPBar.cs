using GameNet;
using UnityEngine;
using UnityEngine.UI;

namespace InGame.Game.HP
{
    public class HPBar : MonoBehaviour
    {
        public HPManager manager;

        public Text nickText;
        public Slider hpbar;
        public Image hpFilling, hpTip;

        public RectTransform lowHpZone;

        private readonly float maxHP = 100;
        private readonly float lowHPThreshold = 40;

        [Header("Colors")]
        public Color32 highColor;
        public Color32 lowColor;


        private string PlayerNick => manager.playerNick;
        private float HP => manager.HP;
        private float smoothHP;



        private void Start()
        {
            hpbar.maxValue = maxHP;

            lowHpZone.anchorMax = new Vector2(lowHPThreshold / maxHP, 1);
            lowHpZone.sizeDelta = Vector2.zero;
        }


        private void Update()
        {
            int HPInt = Mathf.CeilToInt(HP);

            float minHP = Mathf.Min(HP, smoothHP);
            float maxHP = Mathf.Max(HP, smoothHP);
            float lerpedHP = maxHP == 0 ? 0 : minHP / maxHP;

            smoothHP = Mathf.Lerp(HP, smoothHP, lerpedHP);

            nickText.text = $"<b>{PlayerNick}</b> <color=#888>{HPInt}/100</color>";
            hpbar.value = smoothHP;


            Color32 color = GetColorByHp();

            hpFilling.color = color;
            hpTip.color = color;
        }

        private Color32 GetColorByHp()
        {
            if (HP > lowHPThreshold)
            {
                return highColor;
            }

            return lowColor;
        }
    }
}
