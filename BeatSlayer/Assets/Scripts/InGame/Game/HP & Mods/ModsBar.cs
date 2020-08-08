using InGame.Helpers;
using InGame.Menu.Mods;
using System.Collections.Generic;
using UnityEngine;

namespace InGame.Game.Mods
{
    public class ModsBar : MonoBehaviour
    {
        public Transform content;

        public void Refresh(List<ModSO> mods)
        {
            HelperUI.FillContent<ModsBarItem, ModSO>(content, mods, (item, mod) =>
            {
                item.Refresh(mod);
            });
        }
    }
}
