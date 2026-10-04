using UnityEngine;

// Fades this obstacle's material's alpha cutoff when its own box collider blocks the line of sight between the camera and the target.
[RequireComponent(typeof(Renderer))]
[RequireComponent(typeof(BoxCollider))]
public class FadeObstacle : MonoBehaviour
{

    const string CutoffPropertyName = "_Cutoff";

    private Transform target;
    private Transform cameraTransform;
    [SerializeField] private float fadeDuration = 0.3f;
    [SerializeField] private float transparentAlpha = 1f;

    private static readonly int CutoffID = Shader.PropertyToID(CutoffPropertyName);

    private Material _material;
    private BoxCollider _boxCollider;
    private float _opaqueAlpha;
    private float _startAlpha;
    private float _targetAlpha;
    private float _fadeTimer;
    private bool _isFading;
    private bool _isHidden;

    private void Awake()
    {
        _material = GetComponent<Renderer>().material;
        _boxCollider = GetComponent<BoxCollider>();
        _opaqueAlpha = _material.GetFloat(CutoffID);
        _targetAlpha = _opaqueAlpha;

        // Fall back to the tagged player/main camera when references aren't assigned in the Inspector.
        if (target == null)
            target = GameObject.FindWithTag("Player")?.transform;
        if (cameraTransform == null && Camera.main != null)
            cameraTransform = Camera.main.transform;
    }

    private void LateUpdate()
    {
        if (target == null || cameraTransform == null)
            return;

        Vector3 dir = target.position - cameraTransform.position;
        bool isBlocking = _boxCollider.Raycast(new Ray(cameraTransform.position, dir.normalized), out _, dir.magnitude);

        if (isBlocking != _isHidden)
        {
            _isHidden = isBlocking;
            StartFade(isBlocking ? transparentAlpha : _opaqueAlpha);
        }
    }

    private void Update()
    {
        if (!_isFading)
            return;

        _fadeTimer += Time.deltaTime;

        if (_fadeTimer >= fadeDuration)
        {
            SetAlpha(_targetAlpha);
            _fadeTimer = 0f;
            _isFading = false;
            return;
        }

        float t = _fadeTimer / fadeDuration;
        SetAlpha(Mathf.Lerp(_startAlpha, _targetAlpha, t));
    }

    public void StartFade(float targetAlpha)
    {
        _startAlpha = _material.GetFloat(CutoffID);
        _targetAlpha = targetAlpha;
        _fadeTimer = 0f;
        _isFading = true;
    }

    private void SetAlpha(float alpha)
    {
        _material.SetFloat(CutoffID, alpha);
    }

    public float GetOpaqueAlpha()
    {
        return _opaqueAlpha;
    }
    
    public float GetTransparentAlpha()
    {
        return transparentAlpha;
    }
}
