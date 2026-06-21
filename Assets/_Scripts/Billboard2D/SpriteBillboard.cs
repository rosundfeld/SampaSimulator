using UnityEngine;

public class SpriteBillboard : MonoBehaviour
{

    [SerializeField] private bool fixedXZ = true;
    void LateUpdate()
    {
        if (fixedXZ)
        {
            transform.rotation = Quaternion.Euler(0, Camera.main.transform.rotation.eulerAngles.y, 0);
        }
        else
        {
            transform.rotation = Camera.main.transform.rotation;
        }
    }
}
