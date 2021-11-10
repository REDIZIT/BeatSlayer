using Newtonsoft.Json;
using System.IO;

namespace InGame.Utils
{
	public static class FileLoader
	{
		public static T LoadJson<T>(string filepath)
        {
			if (File.Exists(filepath) == false) throw new System.Exception($"Can't load json: File not found at '{filepath}'");

			string json = File.ReadAllText(filepath);
			return JsonConvert.DeserializeObject<T>(json);
        }

		public static void SaveJson<T>(string filepath, T data)
        {
			string json = JsonConvert.SerializeObject(data, Formatting.Indented);
			File.WriteAllText(filepath, json);
		}
	}
}