using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

public class SSytem : MonoBehaviour
{
    public static SSytem instance;

    string filepath;


    public Dictionary<string, string> data;

    #region Save vars

    // Var type              Encoding
    // string,int,bool.......Name|Value
    // Dict<>................Name|Key1;Value1|Key2;Value2
    // Color.................Name|r,g,b,a


    string defaultSave =
@"bloomQuality|2
fileLoad|false
console|false
showfps|false
leftCubeColor|0;0.9;1
rightCubeColor|1;0.5;0
leftDirColor|1;1;1
rightDirColor|1;1;1
NoArrows|False
NoLines|False
CubesSpeed|10
MusicSpeed|10
GlowQuality|2
TrackTextSide|0
GlowPower|2
SliceSound|True
SliceVolume|4
MenuMusic|True
MenuMusicVolume|1
FingerPause|0
Vibration|1
KickVideo|True
EnableFileLoad|0
EnableConsole|0
EnableFps|0";

    public int score { get { return GetInt("score"); } set { SetInt("score", value); } }

    public bool postProcessing { get { return GetBool("postProcessing"); } set { SetBool("postProcessing", value); } }
    public int bloomQuality { get { return GetInt("bloomQuality"); } set { SetInt("bloomQuality", value); } }
    public bool fileLoad { get { return GetBool("fileLoad"); } set { SetBool("fileLoad", value); } }
    public bool console { get { return GetBool("console"); } set { SetBool("console", value); } }
    public bool showfps { get { return GetBool("showfps"); } set { SetBool("showfps", value); } }

    public Color leftColor { get { return GetColor("leftCubeColor"); } set { SetColor("leftCubeColor", value.r, value.g, value.b); } }
    public Color rightColor { get { return GetColor("rightCubeColor"); } set { SetColor("rightCubeColor", value.r, value.g, value.b); } }
    public Color leftDirColor { get { return GetColor("leftDirColor"); } set { SetColor("leftDirColor", value.r, value.g, value.b); } }
    public Color rightDirColor { get { return GetColor("rightDirColor"); } set { SetColor("rightDirColor", value.r, value.g, value.b); } }

    #endregion

    private void Awake()
    {
        instance = this;

        filepath = Application.persistentDataPath + "/ssave.txt";

        data = new Dictionary<string, string>();

        ReadFile();
    }

    void CheckFile()
    {
        if (!File.Exists(filepath))
        {
            File.WriteAllText(filepath, defaultSave);
        }
    }


    public void ReadFile(int splitCount = 2)
    {
        CheckFile();

        string[] lines = File.ReadAllLines(filepath);

        foreach (string line in lines)
        {
            string[] split = line.Split(new char[1] { '|' }, splitCount);

            if (split.Length <= 1) continue;

            string key = split[0];
            string value = split[1];

            data.Add(key, value);
        }

        // Проверяем есть ли новые параметры в обновлении
        bool doSave = false;
        foreach (string line in defaultSave.Split('\n'))
        {
            string[] split = line.Split('|');
            string defaultKey = split[0];

            if (!data.ContainsKey(defaultKey))
            {
                data.Add(defaultKey, split[1]);
                doSave = true;
            }
        }
        if (doSave) SaveFile();
    }

    public void SaveFile()
    {
        List<string> lines = new List<string>();
        foreach (var item in data)
        {
            string line = item.Key + "|" + item.Value;
            lines.Add(line);
        }
        File.WriteAllLines(filepath, lines.ToArray());
    }






    public string GetString(string key)
    {
        if (data.ContainsKey(key)) return data[key];
        else
        {
            Debug.LogError("Try get var with key " + key + " but value is empty");
            return "";
        }
    }

    public void SetString(string key, string value)
    {
        bool exists = data.ContainsKey(key);
        if (exists)
        {
            data[key] = value;
        }
        else
        {
            data.Add(key, value);
        }

        SaveFile();
    }



    #region Int, Float, Bool

    public int GetInt(string key)
    {
        try
        {
            return int.Parse(GetString(key));
        }
        catch(Exception err)
        {
            if (key == "True") return 1;
            else if (key == "False") return 0;
            Debug.LogError("ERROR FOR KEY " + key);
            return 0;
        }
    }
    public void SetInt(string key, int value)
    {
        SetString(key, value.ToString());
    }

    public float GetFloat(string key)
    {
        return float.Parse(GetString(key));
    }
    public void SetFloat(string key, float value)
    {
        SetString(key, value.ToString());
    }

    public bool GetBool(string key)
    {
        try
        {
            string value = GetString(key);
            if (value == "0") return false;
            else if (value == "1") return true;
            return bool.Parse(GetString(key));
        }
        catch
        {
            Debug.LogError("[SSYSTEM] Bool key: " + key);
            return false;
        }
    }
    public void SetBool(string key, bool value)
    {
        SetString(key, value.ToString());
    }

    #endregion

    #region Dictionary

    public Dictionary<string, string> GetDictionary(string key)
    {
        Dictionary<string, string> dict = new Dictionary<string, string>();

        // Dict string example
        // %DictName%|%DictKeys%
        // В DictKeys
        // %Key%;%Value%|%Key%;%Value%

        if (!data.ContainsKey(key))
        {
            Debug.LogError("Try get dict with key " + key + " but value is empty");
        }
        else
        {
            string value = GetString(key);
            string[] dictItems = value.Split('|');
            foreach (string item in dictItems)
            {
                string[] split = item.Split(';');
                dict.Add(split[0], split[1]);
            }
        }

        return dict;
    }
    public void SetDictionary(string key, Dictionary<string, string> dict)
    {
        string dictStr = "";

        var pairs = dict.ToArray();
        for (int i = 0; i < pairs.Length; i++)
        {
            dictStr += (i == 0 ? "" : "|") + pairs[i].Key + ";" + pairs[i].Value;
        }

        SetString(key, dictStr);
    }

    public Dictionary<string, int> GetDictionaryInt(string key)
    {
        Dictionary<string, int> dict = new Dictionary<string, int>();
        foreach (var pair in GetDictionary(key))
        {
            dict.Add(pair.Key, int.Parse(pair.Value));
        }
        return dict;
    }
    public void SetDictionary(string key, Dictionary<string, int> dict)
    {
        Dictionary<string, string> dictStr = new Dictionary<string, string>();
        foreach (var pair in dict)
        {
            dictStr.Add(pair.Key, pair.Value.ToString());
        }
        SetDictionary(key, dictStr);
    }

    #endregion

    #region Color

    public void SetColor(string key, float r, float g, float b)
    {
        SetString(key, r + ";" + g + ";" + b);
    }
    public Color GetColor(string key)
    {
        string[] split = GetString(key).Split(';');
        float r = float.Parse(split[0]);
        float g = float.Parse(split[1]);
        float b = float.Parse(split[2]);
        //float a = float.Parse(split[3]);
        return new Color(r, g, b/*, a*/);
    }

    #endregion


    #region Security

    public string Encrypt(string str)
    {
        return StringCipher.Encrypt(str, "What are you doing here?");
    }
    public string Decrypt(string str)
    {
        return StringCipher.Decrypt(str, "What are you doing here?");
    }

    #endregion


}

public static class StringCipher
{
    // This constant is used to determine the keysize of the encryption algorithm in bits.
    // We divide this by 8 within the code below to get the equivalent number of bytes.
    private const int Keysize = 256;

    // This constant determines the number of iterations for the password bytes generation function.
    private const int DerivationIterations = 1000;

    public static string Encrypt(string plainText, string passPhrase)
    {
        // Salt and IV is randomly generated each time, but is preprended to encrypted cipher text
        // so that the same Salt and IV values can be used when decrypting.  
        var saltStringBytes = Generate256BitsOfRandomEntropy();
        var ivStringBytes = Generate256BitsOfRandomEntropy();
        var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
        using (var password = new Rfc2898DeriveBytes(passPhrase, saltStringBytes, DerivationIterations))
        {
            var keyBytes = password.GetBytes(Keysize / 8);
            using (var symmetricKey = new RijndaelManaged())
            {
                symmetricKey.BlockSize = 256;
                symmetricKey.Mode = CipherMode.CBC;
                symmetricKey.Padding = PaddingMode.PKCS7;
                using (var encryptor = symmetricKey.CreateEncryptor(keyBytes, ivStringBytes))
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        using (var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                        {
                            cryptoStream.Write(plainTextBytes, 0, plainTextBytes.Length);
                            cryptoStream.FlushFinalBlock();
                            // Create the final bytes as a concatenation of the random salt bytes, the random iv bytes and the cipher bytes.
                            var cipherTextBytes = saltStringBytes;
                            cipherTextBytes = cipherTextBytes.Concat(ivStringBytes).ToArray();
                            cipherTextBytes = cipherTextBytes.Concat(memoryStream.ToArray()).ToArray();
                            memoryStream.Close();
                            cryptoStream.Close();
                            return Convert.ToBase64String(cipherTextBytes);
                        }
                    }
                }
            }
        }
    }

    public static string Decrypt(string cipherText, string passPhrase)
    {
        // Get the complete stream of bytes that represent:
        // [32 bytes of Salt] + [32 bytes of IV] + [n bytes of CipherText]
        var cipherTextBytesWithSaltAndIv = Convert.FromBase64String(cipherText);
        // Get the saltbytes by extracting the first 32 bytes from the supplied cipherText bytes.
        var saltStringBytes = cipherTextBytesWithSaltAndIv.Take(Keysize / 8).ToArray();
        // Get the IV bytes by extracting the next 32 bytes from the supplied cipherText bytes.
        var ivStringBytes = cipherTextBytesWithSaltAndIv.Skip(Keysize / 8).Take(Keysize / 8).ToArray();
        // Get the actual cipher text bytes by removing the first 64 bytes from the cipherText string.
        var cipherTextBytes = cipherTextBytesWithSaltAndIv.Skip((Keysize / 8) * 2).Take(cipherTextBytesWithSaltAndIv.Length - ((Keysize / 8) * 2)).ToArray();

        using (var password = new Rfc2898DeriveBytes(passPhrase, saltStringBytes, DerivationIterations))
        {
            var keyBytes = password.GetBytes(Keysize / 8);
            using (var symmetricKey = new RijndaelManaged())
            {
                symmetricKey.BlockSize = 256;
                symmetricKey.Mode = CipherMode.CBC;
                symmetricKey.Padding = PaddingMode.PKCS7;
                using (var decryptor = symmetricKey.CreateDecryptor(keyBytes, ivStringBytes))
                {
                    using (var memoryStream = new MemoryStream(cipherTextBytes))
                    {
                        using (var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
                        {
                            var plainTextBytes = new byte[cipherTextBytes.Length];
                            var decryptedByteCount = cryptoStream.Read(plainTextBytes, 0, plainTextBytes.Length);
                            memoryStream.Close();
                            cryptoStream.Close();
                            return Encoding.UTF8.GetString(plainTextBytes, 0, decryptedByteCount);
                        }
                    }
                }
            }
        }
    }

    private static byte[] Generate256BitsOfRandomEntropy()
    {
        var randomBytes = new byte[32]; // 32 Bytes will give us 256 bits.
        using (var rngCsp = new RNGCryptoServiceProvider())
        {
            // Fill the array with cryptographically secure random bytes.
            rngCsp.GetBytes(randomBytes);
        }
        return randomBytes;
    }
}