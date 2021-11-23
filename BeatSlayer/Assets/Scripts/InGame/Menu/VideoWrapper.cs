using UnityEngine;
using UnityEngine.UI;

namespace InGame.Menu
{
    [ExecuteInEditMode]
    public class VideoWrapper : MonoBehaviour
    {
        [SerializeField] private CanvasScaler canvasScaler;

        private RectTransform rect;
        private float aspectRatio;

        private void Start()
        {
            rect = GetComponent<RectTransform>();
        }
        private void Update()
        {
            float max = Mathf.Max(Screen.width, Screen.height);
            float min = Mathf.Min(Screen.width, Screen.height);

            aspectRatio = Screen.height / (float)Screen.width;

            rect.sizeDelta = new Vector2(max, min);
            transform.localScale = (Vector3.one / canvasScaler.transform.localScale.x) * (Screen.height > Screen.width ? aspectRatio : 1);
        }
    }
}