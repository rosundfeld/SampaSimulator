using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    private const string SAVE_FILE_NAME = "save.json";

    public static SaveManager Instance { get; private set; }

    private readonly List<IDataPersistence> dataPersistenceList = new List<IDataPersistence>();

    private string SavePath => Path.Combine(Application.persistentDataPath, SAVE_FILE_NAME);

    private void Awake()
    {
        Instance = this;
    }

    public void Register(IDataPersistence dataPersistence)
    {
        if (!dataPersistenceList.Contains(dataPersistence))
            dataPersistenceList.Add(dataPersistence);
    }

    public void SaveGame()
    {
        SaveData saveData = new SaveData();

        foreach (IDataPersistence dataPersistence in dataPersistenceList)
            dataPersistence.SaveData(saveData);

        File.WriteAllText(SavePath, JsonUtility.ToJson(saveData, true));
        Debug.Log($"Game saved to {SavePath}");
    }

    public void LoadGame()
    {
        if (!File.Exists(SavePath))
        {
            Debug.LogWarning($"Save file not found at {SavePath}");
            return;
        }

        SaveData saveData = JsonUtility.FromJson<SaveData>(File.ReadAllText(SavePath));

        foreach (IDataPersistence dataPersistence in dataPersistenceList)
            dataPersistence.LoadData(saveData);

        Debug.Log($"Game loaded from {SavePath}");
    }
}
