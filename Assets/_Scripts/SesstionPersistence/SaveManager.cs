using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Collections;

public class SaveManager : MonoBehaviour
{

    private readonly List<IDataPersistence> dataPersistenceObjects = new();
    private string SavePath => Path.Combine(Application.persistentDataPath, "save.json");

    void Start() => StartCoroutine(LoadAfterInit());
    void Awake() => SaveGame();
    

    private IEnumerator LoadAfterInit()
    {
        yield return null;
        //LoadGame();
    }

    public void Register(IDataPersistence dataPersistence)
    {
        if (!dataPersistenceObjects.Contains(dataPersistence))
            dataPersistenceObjects.Add(dataPersistence);
    }
    public void SaveGame()
    {
        SaveData saveData = new();

        foreach (var obj in dataPersistenceObjects)
            obj.SaveData(saveData);

        string json = JsonUtility.ToJson(saveData, true);
        File.WriteAllText(SavePath, json);
        Debug.Log($"Game saved to {SavePath}");
    }

    public void LoadGame()
    {
        if (!File.Exists(SavePath))
        {
            Debug.LogWarning($"Save file not found at {SavePath}");
            return;
        }

        string json = File.ReadAllText(SavePath);
        SaveData saveData = JsonUtility.FromJson<SaveData>(json);

        foreach (var obj in dataPersistenceObjects)
            obj.LoadData(saveData);

        Debug.Log($"Game loaded from {SavePath}");
    }
}
