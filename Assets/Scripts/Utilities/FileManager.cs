using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

public class FileManager
{
    public static bool SaveFile(string saveName, object saveData)
    {
        BinaryFormatter formatter = GetBinaryFormatter();

        if (!Directory.Exists(Application.persistentDataPath + "/Saves"))
        {
            Directory.CreateDirectory(Application.persistentDataPath + "/Saves");
        }

        string path = Application.persistentDataPath + "/Saves/" + saveName + ".sbsv";
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
