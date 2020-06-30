using System.Collections.Generic;
using UnityEngine;

namespace InGame.ScriptableObjects
{
    /// <summary>
    /// Database for all scriptable objects in game. To access it, just use instance.
    /// </summary>
    [CreateAssetMenu(menuName = "Shop/SODB")]
    public class SODB : ScriptableObject
    {
        public List<SaberSO> sabers;
    }
}