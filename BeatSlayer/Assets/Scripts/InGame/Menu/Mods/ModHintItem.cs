using UnityEngine;
using UnityEngine.UI;

namespace InGame.Menu.Mods
{
    public class ModHintItem : MonoBehaviour
    {
        public Image pillowImage;
        public Text pillowText, nameText, descriptionText;

        public void Refresh(ModSO mod)
        {
            pillowImage.color = mod.modPillowColor;
            pillowText.text = mod.shortname;

            nameText.text = mod.name;

            descriptionText.text = mod.description;
            descriptionText.text += $"\n<color=#F40>Score x{mod.scoreMultiplier}</color>";
            descriptionText.text += $"\n<color=#04F>RP x{mod.rpMultiplier}</color>";
        }
    }
}
