using InGame.Game.Scoring.Mods;
using UnityEngine;

namespace InGame.Menu.Mods
{
    [CreateAssetMenu(menuName = "Mod")]
    public class ModSO : ScriptableObject
    {
        public new string name;
        public string shortname, sublabel;
        [TextArea]
        public string description;
        public ModEnum modEnum;

        [Header("UI")]
        public Color modPillowColor;

        [Header("Bonuses")]
        public float scoreMultiplier;
        public float rpMultiplier;

        

        /// <summary>
        /// Add flag into ModEnum
        /// </summary>
        /// <param name="source">ModEnum where mod should flag him</param>
        /// <returns>ModEnum with flagged mod</returns>
        public ModEnum ApplyEnum(ModEnum source)
        {
            source |= modEnum;
            return source;
        }
    }
}
