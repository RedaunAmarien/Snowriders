using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

public class FileManager
{
    public static bool SaveFile(string saveName, object saveData, string saveDirectory)
    {
        BinaryFormatter formatter = GetBinaryFormatter();

        if (!Directory.Exists(saveDirectory))
        {
            Directory.CreateDirectory(saveDirectory);
        }

        string path = saveDirectory + "/" + saveName + ".srd";
        FileStream file = File.Create(path);

        formatter.Serialize(file, saveData);

        file.Close();

        return true;
    }

    public static object LoadFile(string path) {

        if (!File.Exists(path))
        {
            Debug.LogErrorFormat("Tried loading nonexistant file at {0}", path);
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

    public static bool DeleteFile(string path) {
        if (!File.Exists(path)) 
        {
            Debug.LogErrorFormat("Tried deleting nonexistant file at {0}", path);
            return false;
        }
        else {
            try {
                File.Delete(path);
                return true;
            }
            catch {
                Debug.LogErrorFormat("Failed to delete file at {0}", path);
                return false;
            }
        }
    }

    public static BinaryFormatter GetBinaryFormatter() {
        BinaryFormatter formatter = new BinaryFormatter();

        return formatter;
    }
}
