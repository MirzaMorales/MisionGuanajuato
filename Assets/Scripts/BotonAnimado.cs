using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Animación de los botones del menú: al seleccionarlos (o pasar el mouse) crecen un poco
// y aparece una fresa a la izquierda que rebota, para que siempre se vea cuál está elegido.
// Funciona con el tiempo detenido (pausa, Game Over)
public class BotonAnimado : MonoBehaviour, ISelectHandler, IDeselectHandler, IPointerEnterHandler
{
    private const float EscalaSeleccion = 1.07f;

    private Selectable selectable;
    private RectTransform indicador;
    private bool seleccionado;
    private float escala = 1f;

    void Awake()
    {
        selectable = GetComponent<Selectable>();

        RectTransform rt = (RectTransform)transform;
        Image fresa = InterfazUI.Imagen(transform, InterfazUI.CargarSprite("UI/fresa"), "Indicador");
        indicador = fresa.rectTransform;
        float alto = rt.sizeDelta.y;
        InterfazUI.Colocar(indicador, new Vector2(0f, 0.5f), new Vector2(-alto * 0.6f, 0), new Vector2(alto * 0.5f, alto * 0.65f));
        indicador.gameObject.SetActive(false);
    }

    public void OnSelect(BaseEventData datos)
    {
        seleccionado = true;
        indicador.gameObject.SetActive(true);
    }

    public void OnDeselect(BaseEventData datos)
    {
        seleccionado = false;
        indicador.gameObject.SetActive(false);
    }

    // Pasar el mouse selecciona el botón: así el mouse y el teclado muestran lo mismo
    public void OnPointerEnter(PointerEventData datos)
    {
        if (selectable != null && selectable.interactable && EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(gameObject);
        }
    }

    void OnDisable()
    {
        seleccionado = false;
        escala = 1f;
        transform.localScale = Vector3.one;
        if (indicador != null) indicador.gameObject.SetActive(false);
    }

    void Update()
    {
        float objetivo = seleccionado ? EscalaSeleccion : 1f;
        escala = Mathf.MoveTowards(escala, objetivo, Time.unscaledDeltaTime * 0.8f);
        transform.localScale = new Vector3(escala, escala, 1f);

        if (seleccionado)
        {
            Vector2 p = indicador.anchoredPosition;
            p.x = -((RectTransform)transform).sizeDelta.y * 0.6f + Mathf.Sin(Time.unscaledTime * 7f) * 8f;
            indicador.anchoredPosition = p;
        }
    }
}
