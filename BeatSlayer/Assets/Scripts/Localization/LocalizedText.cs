using UnityEngine;
using UnityEngine.UI;

namespace Assets.SimpleLocalization
{
    [RequireComponent(typeof(Text))]
    public class LocalizedText : MonoBehaviour
    {
        public string LocalizationKey;

        [TextArea]
        public string stringToTranslate;

        public string langString;

        public void OnValidate()
        {
            if(langString != null && stringToTranslate != null) langString = stringToTranslate.Replace("\n", "[N]").Replace(",", "[comma]");
        }

        public void Start()
        {
            Localize();
            LocalizationManager.LocalizationChanged += Localize;
        }

        public void OnDestroy()
        {
            LocalizationManager.LocalizationChanged -= Localize;
        }

        private void Localize()
        {
            GetComponent<Text>().text = LocalizationManager.Localize(LocalizationKey);
        }
    }
}