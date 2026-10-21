using UnityEngine;

// Fades this obstacle's material's alpha cutoff when its own box collider blocks the line of sight between the camera and the player.
[RequireComponent(typeof(Renderer))]
[RequireComponent(typeof(BoxCollider))]
public class FadeObstacle : MonoBehaviour
{
    private const string PLAYER_TAG = "Player";

    private static readonly int CUTOFF_ID = Shader.PropertyToID("_Cutoff");

    [SerializeField] private float fadeDuration = 0.3f;
    [SerializeField] private float transparentAlpha = 1f;

    private Transform target;
    private Transform cameraTransform;
    private Material material;
    private BoxCollider boxCollider;
    private float opaqueAlpha;
    private float startAlpha;
    private float targetAlpha;
    private float fadeTimer;
    private bool isFading;
    private bool isHidden;

    private void Awake()
    {
        material = GetComponent<Renderer>().material;
        boxCollider = GetComponent<BoxCollider>();
        opaqueAlpha = material.GetFloat(CUTOFF_ID);
        targetAlpha = opaqueAlpha;
    }

    private void Start()
    {
        // Prefab de cenário não consegue referenciar o player/câmera da cena pelo Inspector.
        target = GameObject.FindWithTag(PLAYER_TAG)?.transform;
        if (Camera.main != null)
            cameraTransform = Camera.main.transform;
    }

    private void LateUpdate()
    {
        if (target == null || cameraTransform == null)
            return;

        Vector3 dir = target.position - cameraTransform.position;
        bool isBlocking = boxCollider.Raycast(new Ray(cameraTransform.position, dir.normalized), out _, dir.magnitude);

        if (isBlocking != isHidden)
        {
            isHidden = isBlocking;
            StartFade(isBlocking ? transparentAlpha : opaqueAlpha);
        }
    }

    private void Update()
    {
        if (!isFading)
            return;

        fadeTimer += Time.deltaTime;

        if (fadeTimer >= fadeDuration)
        {
            SetAlpha(targetAlpha);
            fadeTimer = 0f;
            isFading = false;
            return;
        }

        float t = fadeTimer / fadeDuration;
        SetAlpha(Mathf.Lerp(startAlpha, targetAlpha, t));
    }

    private void StartFade(float newTargetAlpha)
    {
        startAlpha = material.GetFloat(CUTOFF_ID);
        targetAlpha = newTargetAlpha;
        fadeTimer = 0f;
        isFading = true;
    }

    private void SetAlpha(float alpha)
    {
        material.SetFloat(CUTOFF_ID, alpha);
    }
}
