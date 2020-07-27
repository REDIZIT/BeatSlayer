using UnityEngine;

namespace InGame.Game.Sabers
{
    public class SaberModelController : MonoBehaviour
    {
        public Transform trailTop, trailBottom;

        public MeshRenderer[] colorChangableMeshes;

        public void SetColor(Color clr)
        {
            foreach (MeshRenderer mesh in colorChangableMeshes)
            {
                foreach (var mat in mesh.materials)
                {
                    if (!mat.name.ToLower().Contains("saber")) continue;
                    mat.SetColor("_EmissionColor", clr);
                }
            }
        }
    }
}
