using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

//public class PrefsManager : MonoBehaviour
//{
//    [HideInInspector]
//    public Prefs prefs;

//    private void Awake()
//    {
//        Init();
//    }


//    public void Init() {

//        if (!File.Exists(Application.persistentDataPath + "/Prefs.bin"))
//        {
//            Debug.Log("Prefs.bin file not found");
//            prefs = new Prefs();
//            Save();
//        }
//        else
//        {
//            prefs = Load();
//        }
//    }

//    Prefs Load()
//    {
//        BinaryFormatter binaryFormatter = new BinaryFormatter();
//        using (var fileStream = File.Open(Application.persistentDataPath + "/Prefs.bin", FileMode.Open))
//        {
//            Prefs loaded = (Prefs)binaryFormatter.Deserialize(fileStream);


//            // New tech

//            Prefs toreturn = new Prefs();

//            Type loadedType = loaded.GetType();
//            List<FieldInfo> loadedFields = new List<FieldInfo>();
//            loadedFields.AddRange(loadedType.GetFields());

//            Type toreturnType = toreturn.GetType();
//            List<FieldInfo> toreturnFields = new List<FieldInfo>();
//            toreturnFields.AddRange(toreturnType.GetFields());


//            for (int i = 0; i < loadedFields.Count; i++)
//            {
//                // Ищем поле в новой версии файла с названием из прошлой
//                FieldInfo field = toreturnFields.Find(c => c.Name == loadedFields[i].Name);

//                // Берём значение из старого файла
//                object value = loadedFields[i].GetValue(loaded);
//                // Если там null, то загружаем дефолтное значение
//                if(value == null)
//                {
//                    value = loadedFields[i].GetValue(toreturn);
//                }

//                // Устанавливаем в выходной класс значение из файла
//                field.SetValue(toreturn, value);
//            }

//            return toreturn;
//        }
//    }

//    public void Save()
//    {
//        BinaryFormatter binaryFormatter = new BinaryFormatter();
//        using (var fileStream = File.Create(Application.persistentDataPath + "/Prefs.bin"))
//        {
//            binaryFormatter.Serialize(fileStream, prefs);
//        }
//    }
//}
