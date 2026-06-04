using UnityEngine;
using UnityEngine.EventSystems;

public interface IHoverable
{
	void OnHoverEnter();
	void OnHoverExit();
}

public class EventClick : MonoBehaviour, IPointerClickHandler, IPointerUpHandler, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("Objeto clicado: " + gameObject.name);
		Debug.Log("Ponteiro saiu do objeto: " + eventData);
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
        Debug.Log("Ponteiro entrou no objeto: " + gameObject.name);
	}

    public void OnPointerExit(PointerEventData eventData)
    {
        Debug.Log("Ponteiro saiu do objeto: " + gameObject.name);
		Debug.Log("Ponteiro saiu do objeto: " + eventData);
	}
}