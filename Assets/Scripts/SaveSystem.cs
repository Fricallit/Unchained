using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class SaveSystem : MonoBehaviour
{
    private readonly string saveFile = "savedata.dat";

    public void Save(SaveData data)
    {
        string path = Application.persistentDataPath + "/saves/";
        Directory.CreateDirectory(path);
        BinaryFormatter binaryFormatter = new BinaryFormatter();
        FileStream fileStream = File.Open(path + saveFile, FileMode.Create);
        binaryFormatter.Serialize(fileStream, data);
        fileStream.Close();
    }

    public bool SaveExists()
    {
        if (File.Exists(Application.persistentDataPath + "/saves/" + saveFile))
            return true;
        else
            return false;
    }

    public void DeleteSave()
    {
        if (SaveExists())
        {
            string path = Application.persistentDataPath + "/saves/";
            DirectoryInfo directory = new DirectoryInfo(path);
            directory.Delete(true);
            Directory.CreateDirectory(path);
        }
    }

    public SaveData Load()
    {
        SaveData toLoad = null;
        if (SaveExists())
        {
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            FileStream fileStream = File.Open(Application.persistentDataPath + "/saves/" + saveFile, FileMode.Open);

            toLoad = (SaveData)binaryFormatter.Deserialize(fileStream);
            fileStream.Close();
        }
        return toLoad;
    }
}
