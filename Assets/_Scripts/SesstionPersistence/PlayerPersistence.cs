using UnityEngine;

public class PlayerPersistence : MonoBehaviour, IDataPersistence
{

    public SaveManager saveManager;

    void Awake()
    {
        saveManager.Register(this);
    }
    
    public void LoadData(SaveData data)
    {
        transform.position = data.playerPosition;
    }

    public void SaveData(SaveData data)
    {
        data.playerPosition = transform.position;
    }
}
