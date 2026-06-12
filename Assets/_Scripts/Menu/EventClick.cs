using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public interface IHoverable
{
    void OnHoverEnter();
    void OnHoverExit();
}

public static class MenuItem
{
    public const string Jogar = "Jogar";
    public const string Sair = "Sair";
    public const string Creditos = "Creditos";
}

public class EventClick : MonoBehaviour, IPointerClickHandler, IPointerUpHandler, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler
{

    public Material letterMaterial;
    public Material letterHoverMaterial;
    public Material material;
    public Material hoverMaterial;
    public List<GameObject> menuLetters = new List<GameObject>();
    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("Objeto clicado: " + gameObject.name);
        switch (gameObject.name)
        {
            case MenuItem.Jogar:
                // Carrega a cena "City"
                SceneManager.LoadScene("City");
                break;
            case MenuItem.Sair:
                // Fecha o jogo
                Debug.Log("Fechando o jogo");
                Application.Quit();
                break;
            case MenuItem.Creditos:
                // Carrega a cena "Credits"
                SceneManager.LoadScene("Credits");
                break;
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        Debug.Log("Botão do mouse solto no objeto: " + gameObject.name);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        Debug.Log("Botão do mouse pressionado no objeto: " + gameObject.name);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log("OnPointerEnter: " + gameObject.name);
        ApplyMenuMaterials(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Debug.Log("OnPointerExit: " + gameObject.name);
        ApplyMenuMaterials(false);
    }

    private void ApplyMenuMaterials(bool isHover)
    {
        if (menuLetters == null || menuLetters.Count == 0)
        {
            return;
        }

        int lastIndex = menuLetters.Count - 1;

        for (int i = 0; i < menuLetters.Count; i++)
        {
            GameObject letter = menuLetters[i];
            if (letter == null)
            {
                continue;
            }

            Renderer renderer = letter.GetComponent<Renderer>();
            if (renderer == null)
            {
                renderer = letter.GetComponentInChildren<Renderer>();
                if (renderer == null)
                {
                    continue;
                }
            }

            bool isLastItem = i == lastIndex;
            if (isHover)
            {
                renderer.material = isLastItem ? hoverMaterial : letterHoverMaterial;
            }
            else
            {
                renderer.material = isLastItem ? material : letterMaterial;
            }
        }
    }
}