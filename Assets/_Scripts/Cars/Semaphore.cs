using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

// Alterna periodicamente os bloqueios de cada lado do cruzamento (objeto ativo = sinal fechado para os carros).
public class Semaphore : MonoBehaviour
{
    [FormerlySerializedAs("Semaphores")]
    [SerializeField] private List<GameObject> semaphoreList;
    [SerializeField] private float switchInterval = 60f;

    private void Start()
    {
        StartCoroutine(SwitchSemaphoreRoutine());
    }

    private IEnumerator SwitchSemaphoreRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(switchInterval);
            foreach (GameObject semaphore in semaphoreList)
                semaphore.SetActive(!semaphore.activeSelf);
        }
    }
}
