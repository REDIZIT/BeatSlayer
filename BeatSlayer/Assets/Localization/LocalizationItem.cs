using System.Collections.Generic;

namespace Localization
{
    public class LocalizationSheet
    {
        public string name;
        public List<LocalizationItem> dictionary;
    }

    public class LocalizationItem
    {
        public string key;
        public Dictionary<string, string> translations;
    }
}
