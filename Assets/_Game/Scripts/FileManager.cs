using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace Universal.FileManaging
{
    public static class FileManager<T>
    {
        public static void SaveDataToFile(T objectToSave, string dataPath)
        {
            //Create directory if it doesn't exist (only sends path until the last '/')
            Directory.CreateDirectory(dataPath.Substring(0, dataPath.LastIndexOf('/')));

            //Create file
            FileStream file = File.Create(dataPath);

            //Save obj in file and close
            BinaryFormatter bf = new BinaryFormatter();
            bf.Serialize(file, objectToSave);
            file.Close();
        }
        public static T LoadDataFromFile(string dataPath)
        {
            if (!File.Exists(dataPath))
            {
                UnityEngine.Debug.LogWarning("Data file not found at path: " + dataPath);
                return default;
            }

            try
            {
                BinaryFormatter bf = new BinaryFormatter();
                FileStream file = File.Open(dataPath, FileMode.Open);
                T objectToLoad = (T)bf.Deserialize(file);
                file.Close();

                return objectToLoad;
            }
            catch (Exception e)
            {
                return default;
            }
        }
        public static void DeleteFile(string dataPath)
        {
            if (!File.Exists(dataPath)) { return; }

            File.Delete(dataPath);
        }
    }
}