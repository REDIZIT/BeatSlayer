using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.Internal;
using Debug = UnityEngine.Debug;


namespace SaveManagement
{
    public static class SaveManager
    {
        
        public static string FilePath { get { return Application.persistentDataPath + "/data/save.xml"; } }

        private static SaveData data;
        public static SaveData Data
        {
            get
            {
                if (data == null) LoadData();
                return data;
            }
            set
            {
                data = value;
                SaveData();
            }
        }

        public static void LoadData()
        {
            if (File.Exists(FilePath))
            {
                XmlSerializer xml = new XmlSerializer(typeof(SaveData));
                using (Stream s = File.OpenRead(FilePath))
                {
                    data = (SaveData)xml.Deserialize(s);
                }
            }
            else
            {
                data = new SaveData();
                SaveData();
            }
        }
        public static void SaveData()
        {
            Debug.LogError("Save data");
            XmlSerializer xml = new XmlSerializer(typeof(SaveData));
            using (Stream s = File.Create(FilePath))
            {
                xml.Serialize(s, data);
            }
        }
    }

    public class SaveData
    {
        public SaveData() { }

        [XmlIgnore]
        public List<SaberEffectData> SaberEffects
        { 
            get
            {
                if (saberEffects == null) saberEffects =  new List<SaberEffectData>() { new SaberEffectData(0, "", "", 200000, true), new SaberEffectData(1, "", "", 500000, false) };
                return saberEffects;
            } 
            set { saberEffects = value; }
        }

        [SerializeField]
        public List<SaberEffectData> saberEffects;
        
    }

    public class SaberEffectData
    {
        public int spriteID;
        public string nameKey, descriptionKey;

        public float price;
        public bool isBought;

        
        private SaberEffectData() { }
        public SaberEffectData(int spriteID, string nameKey, string descriptionKey, float price, bool isBought)
        {
            this.spriteID = spriteID;
            this.nameKey = nameKey;
            this.descriptionKey = descriptionKey;
            this.price = price;
            this.isBought = isBought;
        }
    }
}