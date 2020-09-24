using UnityEngine;
using UnityEngine.UI;

namespace InGame.UI.Menu.MapsData
{
    public class LikesSlider : MonoBehaviour
    {
        public Text likesText, dislikesText;
        public Slider slider;


        public void Refresh(int likes, int dislikes)
        {
            slider.value = GetSliderValue(likes, dislikes);

            likesText.text = likes.ToString();
            dislikesText.text = dislikes.ToString();
        }

        private float GetSliderValue(int likes, int dislikes)
        {
            if (dislikes == 0) return 1;

            float all = likes + dislikes;
            return likes / all;
        }
    }
}
