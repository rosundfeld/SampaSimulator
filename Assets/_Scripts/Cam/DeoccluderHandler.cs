using Unity.Cinemachine;
using UnityEngine;

public class DeoccluderHandler : MonoBehaviour
{
    public CinemachineCamera vcam;

    void Update()
    {
        var deoccluder = vcam.GetComponent<CinemachineDeoccluder>();
        if (deoccluder == null) return;

        // Assuming DebugIgnoreList is not a valid property, replace it with a valid alternative.
        // If DebugIgnoreList is meant to represent a list of ignored objects, you may need to implement
        // or retrieve such a list from another source. For now, this code assumes a placeholder method.

        //var ignoredObjects = GetIgnoredObjects(deoccluder); // Replace with actual logic to retrieve ignored objects.

        //foreach (var item in ignoredObjects)
        //{
        //    var fade = item.GetComponent<FadeOccluder>();
        //    if (fade) fade.FadeOut();
        //}
    }
}
