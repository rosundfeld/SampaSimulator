using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

// Item 3D do menu principal: a ação é escolhida pelo nome do GameObject (Jogar / Sair / Creditos).
public class EventClick : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    private const string PLAY_ITEM = "Jogar";
    private const string QUIT_ITEM = "Sair";
    private const string CREDITS_ITEM = "Creditos";

    [SerializeField] private Material letterMaterial;
    [SerializeField] private Material letterHoverMaterial;
    [SerializeField] private Material material;
    [SerializeField] private Material hoverMaterial;
    [Tooltip("Letras do item; o ÚLTIMO elemento da lista é o fundo")]
    [SerializeField] private List<GameObject> menuLetters = new List<GameObject>();

    public void OnPointerClick(PointerEventData eventData)
    {
        switch (gameObject.name)
        {
            case PLAY_ITEM:
                Loader.Load(Loader.Scene.Game);
                break;
            case QUIT_ITEM:
                Application.Quit();
                break;
            case CREDITS_ITEM:
                // Cena de créditos ainda não existe.
                break;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        ApplyMenuMaterials(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ApplyMenuMaterials(false);
    }

    private void ApplyMenuMaterials(bool isHover)
    {
        int lastIndex = menuLetters.Count - 1;

        for (int i = 0; i < menuLetters.Count; i++)
        {
            GameObject letter = menuLetters[i];
            if (letter == null)
                continue;

            Renderer letterRenderer = letter.GetComponentInChildren<Renderer>();
            if (letterRenderer == null)
                continue;

            bool isBackground = i == lastIndex;
            if (isHover)
                letterRenderer.material = isBackground ? hoverMaterial : letterHoverMaterial;
            else
                letterRenderer.material = isBackground ? material : letterMaterial;
        }
    }
}
