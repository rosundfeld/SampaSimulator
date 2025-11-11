using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;

public class Semaphore : MonoBehaviour
{
    public List<GameObject> Semaphores;
    public float switchInterval = 60f;
    public bool isSwitching = true;

    private Coroutine switchCoroutine;
    void Start()
    {
        StartSwitching();
    }

    public void StartSwitching()
    {
        if (isSwitching) return;
        isSwitching = true;
        switchCoroutine = StartCoroutine(SwitchSemaphoreRoutine());
    }

    public void StopSwitching()
    {
        if (!isSwitching) return;
        isSwitching = false;

        // Opcionalmente, cancelar imediatamente usando StopCoroutine
        if (switchCoroutine != null)
        {
            StopCoroutine(switchCoroutine);
            switchCoroutine = null;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator SwitchSemaphoreRoutine()
    {
        while (isSwitching)
        {
            yield return new WaitForSeconds(switchInterval);
            foreach (GameObject semaphore in Semaphores)
            {
                semaphore.SetActive(!semaphore.activeSelf);
            }
        }
    }
}
