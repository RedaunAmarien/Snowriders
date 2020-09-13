using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

public class FileManager
{
    public static bool SaveFile(string saveName, object saveData, string saveDirectory)
    {
        BinaryFormatter formatter = GetBinaryFormatter();

        if (!Directory.Exists(Application.persistentDataPath + saveDirectory))
        {
            Directory.CreateDirectory(Application.persistentDataPath + saveDirectory);
        }

        string path = Application.persistentDataPath + saveDirectory + "/" + saveName + ".srd";
        FileStream file = File.Create(path);

        formatter.Serialize(file, saveData);

        file.Close();

        return true;
    }

    public static object LoadFile(string path) {

        if (!File.Exists(path))
        {
            return null;
        }

        BinaryFormatter formatter = GetBinaryFormatter();

        FileStream file = File.Open(path, FileMode.Open);

        try
        {
            object save = formatter.Deserialize(file);
            file.Close();
            return save;
        }
        catch
        {
            Debug.LogErrorFormat("Failed to load file at {0}", path);
            file.Close();
            return null;
        }
    }

    public static BinaryFormatter GetBinaryFormatter() {
        BinaryFormatter formatter = new BinaryFormatter();

        return formatter;
    }
}
