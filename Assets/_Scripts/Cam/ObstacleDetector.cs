using UnityEngine;

public class ObstacleDetector : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private Transform cameraTransform;

    private FadeObstacle _currentFadeObstacle;


    private void LateUpdate()
    {
        // Cast from the camera towards the target so obstacles between them are detected.
        Vector3 dir = target.position - cameraTransform.position;
        if (Physics.Raycast(cameraTransform.position, dir.normalized, out RaycastHit hit, dir.magnitude))
        {
            FadeObstacle fadeObstacle = hit.collider.GetComponent<FadeObstacle>();

            if (fadeObstacle != null && fadeObstacle != _currentFadeObstacle)
            {
                if (_currentFadeObstacle != null)
                    _currentFadeObstacle.StartFade(_currentFadeObstacle.GetOpaqueAlpha());

                _currentFadeObstacle = fadeObstacle;
                _currentFadeObstacle.StartFade(_currentFadeObstacle.GetTransparentAlpha());
            }
        }
        else if (_currentFadeObstacle != null)
        {
            _currentFadeObstacle.StartFade(_currentFadeObstacle.GetOpaqueAlpha());
            _currentFadeObstacle = null;
        }
    }
}

