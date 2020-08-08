using InGame.Menu.Mods;
using UnityEngine;
using UnityEngine.UI;

namespace InGame.Game.Mods
{
    public class ModsBarItem : MonoBehaviour
    {
        public Image pillow;
        public Text nameText;

        public void Refresh(ModSO mod)
        {
            pillow.color = mod.modPillowColor;
            nameText.text = mod.shortname;
        }
    }
}
