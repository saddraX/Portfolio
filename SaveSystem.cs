using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public static class SaveSystem
{
    public static void SaveGame(Player player, GameObject itemsYouCanEquip, int saveIndex, bool savePlayerOrItemsData, bool saveSettingsData, bool savePosition)
    {
        BinaryFormatter formatter = new BinaryFormatter();

        string saveFolderPath = Application.persistentDataPath + "/Saves/" + saveIndex; //save folder path
        if (!Directory.Exists(saveFolderPath)) Directory.CreateDirectory(Application.persistentDataPath + "/Saves/" + saveIndex); //create save folder if not exists

        if (savePlayerOrItemsData) //save only player and items data
        {
            string savePlayerFilePath = Application.persistentDataPath + "/Saves/" + saveIndex + "/player_data.save";
            string savePlayerPositionFilePath = Application.persistentDataPath + "/Saves/" + saveIndex + "/player_position_data.save";
            string saveItemsFilePath = Application.persistentDataPath + "/Saves/" + saveIndex + "/items_data.save";

            FileStream savePlayerFileStream = new FileStream(savePlayerFilePath, FileMode.OpenOrCreate);
            FileStream savePlayerPositionFileStream = new FileStream(savePlayerPositionFilePath, FileMode.OpenOrCreate);
            FileStream saveItemsFileStream = new FileStream(saveItemsFilePath, FileMode.OpenOrCreate);

            PlayerData playerData = new PlayerData(player);
            PlayerPositionData playerPositionData = new PlayerPositionData(player);
            ItemsData itemsData = new ItemsData(itemsYouCanEquip);

            formatter.Serialize(savePlayerFileStream, playerData);
            if (savePosition) formatter.Serialize(savePlayerPositionFileStream, playerPositionData);
            formatter.Serialize(saveItemsFileStream, itemsData);

            savePlayerFileStream.Close();
            savePlayerPositionFileStream.Close();
            saveItemsFileStream.Close();
        }

        if (saveSettingsData) //save only settings
        {
            string saveSettingsFilePath = Application.persistentDataPath + "/Saves/" + saveIndex + "/settings_data.save";

            FileStream saveSettingsFileStream = new FileStream(saveSettingsFilePath, FileMode.OpenOrCreate);

            SettingsData audioData = new SettingsData();

            formatter.Serialize(saveSettingsFileStream, audioData);

            saveSettingsFileStream.Close();

            Debug.Log("Settings has been saved!");
        }
    }

    //loading player data
    public static PlayerData LoadPlayerData(int saveIndex)
    {
        string path = Application.persistentDataPath + "/Saves/" + saveIndex + "/player_data.save";

        if (File.Exists(path))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream fileStream = new FileStream(path, FileMode.Open);

            PlayerData playerData = formatter.Deserialize(fileStream) as PlayerData;

            fileStream.Close();

            return playerData;
        }
        else
        {
            if (FirstFrameStart.alreadyStarted) Debug.LogError("No file found in: " + path);
            return null;
        }
    }
    public static PlayerPositionData LoadPlayerPositionData(int saveIndex)
    {
        string path = Application.persistentDataPath + "/Saves/" + saveIndex + "/player_position_data.save";

        if (File.Exists(path))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream fileStream = new FileStream(path, FileMode.Open);

            PlayerPositionData playerPositionData = formatter.Deserialize(fileStream) as PlayerPositionData;

            fileStream.Close();

            return playerPositionData;
        }
        else
        {
            if (FirstFrameStart.alreadyStarted) Debug.LogError("No file found in: " + path);
            return null;
        }
    }

    //loading items data (items, destinations and collectables)
    public static ItemsData LoadItemsData(int saveIndex)
    {
        string path = Application.persistentDataPath + "/Saves/" + saveIndex + "/items_data.save";

        if (File.Exists(path))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream fileStream = new FileStream(path, FileMode.Open);

            ItemsData itemsData = formatter.Deserialize(fileStream) as ItemsData;

            fileStream.Close();

            return itemsData;
        }
        else
        {
            if (FirstFrameStart.alreadyStarted) Debug.LogError("No file found in: " + path);
            return null;
        }
    }

    //loading settings data
    public static SettingsData LoadSettingsData(int saveIndex)
    {
        string path = Application.persistentDataPath + "/Saves/" + saveIndex + "/settings_data.save";

        if (File.Exists(path))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream fileStream = new FileStream(path, FileMode.Open);

            SettingsData settingsData = formatter.Deserialize(fileStream) as SettingsData;

            fileStream.Close();

            return settingsData;
        }
        else
        {
            if (FirstFrameStart.alreadyStarted) Debug.LogError("No file found in: " + path);
            return null;
        }
    }

    public static void DeleteSaveFiles(int saveIndex)
    {
        string saveFolderPath = Application.persistentDataPath + "/Saves/" + saveIndex;
        if (Directory.Exists(saveFolderPath)) Directory.Delete(Application.persistentDataPath + "/Saves/" + saveIndex, true);
    }
}
