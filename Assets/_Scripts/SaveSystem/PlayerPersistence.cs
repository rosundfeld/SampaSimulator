using UnityEngine;

public class PlayerPersistence : MonoBehaviour, IDataPersistence
{
    private void Start()
    {
        SaveManager.Instance.Register(this);
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
