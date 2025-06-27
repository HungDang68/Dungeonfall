using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public static class SaveSystem
{

    public static void SaveStats(Stats stats)
    {
        BinaryFormatter formatter = new BinaryFormatter();

        string path = Application.persistentDataPath + "/Player.stats";

        using (FileStream fileStream = new FileStream(path, FileMode.Create, FileAccess.Write))
        {
            formatter.Serialize(fileStream, stats);
        }
    }

    public static Stats LoadStats()
    {
        string path = Application.persistentDataPath + "/Player.stats";
        if (File.Exists(path))
        {
            try
            {
                BinaryFormatter formatter = new BinaryFormatter();
                using (FileStream fileStream = new FileStream(path, FileMode.Open))
                {
                    return formatter.Deserialize(fileStream) as Stats;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to load stats: {e.Message}");
                return null;
            }
        }
        else
        {
            Debug.Log("Stat file couldn't be found!!");
            return null;
        }
    }


}
