using UnityEngine;

public class FadeOccluder : MonoBehaviour
{
    public float fadeSpeed = 5f;
    public float fadeAmount = 0.3f;
    float originalOpacity;

    Material mat;
    public bool isFaded = false;

    void Start()
    {
        mat = GetComponent<Renderer>().material;
        originalOpacity = mat.color.a;
    }

    public void FadeOut()
    {
        isFaded = true;
    }

    public void FadeIn()
    {
        isFaded = false;
    }

    private void ResetFade()
    {
        Color currentColor = mat.color;
        Color smoothColor = new Color(currentColor.r, currentColor.g, currentColor.b,
            Mathf.Lerp(currentColor.a, originalOpacity, fadeSpeed * Time.deltaTime));
        mat.color = smoothColor;
    }

    private void FadeNow()
    {
        Color currentColor = mat.color;
        Color smoothColor = new Color(currentColor.r, currentColor.g, currentColor.b,
            Mathf.Lerp(currentColor.a, fadeAmount, fadeSpeed * Time.deltaTime));
        mat.color = smoothColor;
    }

    void Update()
    {
        if (isFaded)
            FadeNow();
        else
            ResetFade();
    }
}
