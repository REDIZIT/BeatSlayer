using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class AdvancedSaveManager : MonoBehaviour
{
    //public AdvancedSave save;
    Prefs _prefs;
    public Prefs prefs
    {
        get
        {
            if (_prefs == null) Load();
            return _prefs;
        }
        set
        {
            _prefs = value;
        }
    }

    BinaryFormatter formatter = new BinaryFormatter();

    const string filename = "/save.bin";
    const string prevFilename = "/Prefs.bin";

    private void Awake()
    {
        Load();
    }

    public void Save()
    {
        //Debug.LogWarning("To save");

        //prefs.leftDirColor = null;
        //prefs.rightDirColor = null;
        using (Stream stream = File.Create(Application.persistentDataPath + filename))
        {
            formatter.Serialize(stream, prefs);
        }

        Type saveType = prefs.GetType(); // Получаем тип загруженного файла (с возможно неполными полями)
        FieldInfo[] fields = saveType.GetFields();

        string msg = "Saved file has these fields:";
        for (int i = 0; i < fields.Length; i++)
        {
            msg += "\n" + fields[i].Name + " = " + fields[i].GetValue(prefs);
        }
    }

    public void SaveDefault()
    {
        using (Stream stream = File.Create(Application.persistentDataPath + filename))
        {
            formatter.Serialize(stream, new AdvancedSave());
        }
    }

    public void Load()
    {
        // Загружен из памяти (возможны неточности)
        Prefs loaded;

        if(!File.Exists(Application.persistentDataPath + filename))
        {
            if(File.Exists(Application.persistentDataPath + prevFilename))
            {
                // Значит обновить файл
                Debug.LogWarning("Updating prefs file");
                File.Move(Application.persistentDataPath + prevFilename, Application.persistentDataPath + filename);
            }
            else
            {
                prefs = new Prefs();
                Save();
                Debug.LogWarning("Sabed");
                return;
            }
        }

        using (Stream stream = File.Open(Application.persistentDataPath + filename, FileMode.Open))
        {
            loaded = (Prefs)formatter.Deserialize(stream);
        }

        // Алгоритм действий

        // Создаём новый экземпляр класса сохранения для соотношения полей (result)

        // Мы должны пробежать по всем полям в загруженном файл и найти соответствующие поля в правильном классе
        // Если мы находим, то меняем стандратное значение у правильного класса на то, которое в загруженном файле (Это просто загрузка, ничего необычного)
        // Если мы не находим поле в правильном (новом экземпляре) классе, то значит, что разраб удалил это поле

        // Далее, а что если разраб добавил новое поле?
        // Для этого мы и создавали новый экземпляр класса. В result изначально все значения стандартные!
        // Т.е. если разраб добавил новое поле, то оно будет в result. Ноо! В loaded его нет, потому что loaded это файл из памяти до обновления

        Prefs result = new Prefs();


        Type saveType = loaded.GetType(); // Получаем тип загруженного файла (с возможно неполными полями)
        FieldInfo[] fields = saveType.GetFields();

        //Debug.Log("Loaded file has these fields: ");
        for (int i = 0; i < fields.Length; i++)
        {
            //Debug.Log("(" + fields[i].FieldType + ") " + fields[i].Name + " = " + (fields[i].GetValue(loaded) == null ? "[ NULL ]" : fields[i].GetValue(loaded)));

            // Получаем значение поля из файла
            object fieldVal = fields[i].GetValue(loaded);

            // Если там есть значение. Т.е. это поле было в пред. версии класса
            if (fieldVal != null)
            {
                // Помещаем в результат значение. (Обычная загрузка)

                // Если тип не array
                if (fieldVal as Array == null)
                {
                    fields[i].SetValue(result, fieldVal);
                }
                else
                {
                    object defaultVal = fields[i].GetValue(result);

                    fields[i].SetValue(result, HandleArray(fieldVal, (Array)defaultVal));
                    //fields[i].SetValue(result, fieldVal);

                    //Debug.LogWarning("Array detected! " + fields[i].Name);
                }
            }
            else
            {
                // Если значение null, то это значит, что разраб добавил новое поле
                // Т.к. в новом экземпляре это поле есть, а в экземпляре файла сохранения нет
                // И как бы нужно установить стандартное значение, но оно уже становленно (AdvancedSave result = new AdvancedSave();)
            }
        }

        prefs = result;

        ViewSaveFields();
    }

    public Array HandleArray(object fieldVal, Array defaultVal)
    {
        // Получаем загруженное значение
        Array loaded = (Array)fieldVal;

        Type arrayElementType = defaultVal.GetValue(0).GetType();

        // Нужно узнать какая разница в размера
        // Если разницы нет, то просто вставляем все элементы в loaded. Т.е. можно просто вернуть loaded
        // Если разраб увеличил длину, то в новые ячейки нужно поместить стандартные значения
        // Если же разраб уменьшил длину, то просто урезать размер

        // Чекаем длину
        if (loaded.Length == defaultVal.Length)
        {
            //Debug.LogWarning("loaded.len = defaultval.len => " + loaded.Length + " = " + defaultVal.Length);
            return loaded;
        }
        else if (loaded.Length < defaultVal.Length)
        {
            // Значит разраб увеличил длину

            // В качестве max используем максимальную длину, т.е. длину defaultVal
            Array newArr = Array.CreateInstance(arrayElementType, defaultVal.Length);
            for (int i = 0; i < defaultVal.Length; i++)
            {
                if(i < loaded.Length)
                {
                    newArr.SetValue(loaded.GetValue(i), i);
                }
                else
                {
                    newArr.SetValue(defaultVal.GetValue(i), i);
                }
            }

            //Debug.LogWarning("loaded.len < defaultval.len => " + loaded.Length + " < " + defaultVal.Length);
            return newArr;
        }
        else
        {
            // Значит разраб уменьшил длину

            // Возвращаем только те значения, i которых не больше defaultVal.Length
            Array newArr = Array.CreateInstance(arrayElementType, defaultVal.Length);
            for (int i = 0; i < defaultVal.Length; i++)
            {
                newArr.SetValue(loaded.GetValue(i), i);
            }

            //Debug.LogWarning("loaded.len > defaultval.len => " + loaded.Length + " > " + defaultVal.Length);
            return newArr;
        }
    }

    public void ViewSaveFields()
    {
        string msg = "Runtime save has these fields:";

        Type saveType = prefs.GetType();
        FieldInfo[] fields = saveType.GetFields();
        for (int i = 0; i < fields.Length; i++)
        {
            msg += "\n" + fields[i].Name + " = " + fields[i].GetValue(prefs);
        }
        //Debug.Log(msg);
    }
}


[Serializable]
public class AdvancedSave
{
    public string name;
    public int age;

    public bool? isCoolBoy = true;

    public bool? isTheBest = true;

    public int[] arr = new int[10];
}