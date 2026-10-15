using TMPro;
using UnityEngine;

// Cartel de tutorial dentro del nivel: un texto breve que aparece cuando el jugador se acerca
// y se desvanece al alejarse. Se coloca como un objeto vacío en la escena; el texto y su fondo
// se crean al iniciar. Sirve para enseñar controles y objetivos en la zona de práctica
public class CartelTutorial : MonoBehaviour
{
    [SerializeField, TextArea] private string mensaje = "Salta con ESPACIO";
    [SerializeField] private float distanciaVisible = 4f;
    [SerializeField] private Vector2 tamano = new Vector2(5.5f, 1.3f);

    private TextMeshPro texto;
    private SpriteRenderer fondo;
    private Transform jugador;
    private float alfa;

    void Start()
    {
        PlayerController pc = FindFirstObjectByType<PlayerController>();
        if (pc != null) jugador = pc.transform;

        // Fondo oscuro detrás del texto
        GameObject goFondo = new GameObject("Fondo");
        goFondo.transform.SetParent(transform, false);
        fondo = goFondo.AddComponent<SpriteRenderer>();
        fondo.sprite = Resources.Load<Sprite>("UI/pixel");
        fondo.color = new Color(0f, 0f, 0f, 0f);
        fondo.sortingOrder = 14;
        // El sprite "pixel" mide 0.125 unidades: se escala al tamaño del cartel más un margen
        goFondo.transform.localScale = new Vector3((tamano.x + 0.3f) / 0.125f, (tamano.y + 0.2f) / 0.125f, 1f);

        // Texto (se ajusta solo al tamaño del cartel)
        GameObject goTexto = new GameObject("Texto");
        goTexto.transform.SetParent(transform, false);
        texto = goTexto.AddComponent<TextMeshPro>();
        texto.rectTransform.sizeDelta = tamano;
        texto.text = mensaje;
        texto.alignment = TextAlignmentOptions.Center;
        texto.enableAutoSizing = true;
        texto.fontSizeMin = 0.5f;
        texto.fontSizeMax = 4f;
        texto.color = new Color(1f, 0.97f, 0.88f, 0f);
        texto.outlineWidth = 0.2f;
        texto.outlineColor = new Color32(27, 20, 32, 255);
        texto.sortingOrder = 15;
    }

    void Update()
    {
        if (texto == null) return;

        bool cerca = jugador != null && Mathf.Abs(jugador.position.x - transform.position.x) < distanciaVisible;
        alfa = Mathf.MoveTowards(alfa, cerca ? 1f : 0f, Time.deltaTime * 3f);

        Color c = texto.color;
        c.a = alfa;
        texto.color = c;
        fondo.color = new Color(0f, 0f, 0f, alfa * 0.55f);
    }
}
