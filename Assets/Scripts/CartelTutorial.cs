using TMPro;
using UnityEngine;

// Cartel de tutorial dentro del nivel: un marco de madera con un texto breve que aparece cuando el
// jugador se acerca (crece un poco y flota) y se desvanece al alejarse. Se coloca como un objeto vacío
// en la escena; el marco y el texto se crean al iniciar. Las teclas pueden resaltarse con
// <color=#FFC832>...</color> dentro del mensaje
public class CartelTutorial : MonoBehaviour
{
    // Escala del marco: con esto el borde de madera mide unas 0.25 unidades
    private const float EscalaMarco = 0.35f;

    [SerializeField, TextArea] private string mensaje = "Salta con ESPACIO";
    [SerializeField] private float distanciaVisible = 4f;
    [SerializeField] private Vector2 tamano = new Vector2(5.5f, 1.3f);

    private TextMeshPro texto;
    private SpriteRenderer marco;
    private Transform contenido;
    private Transform jugador;
    private Vector3 posicionBase;
    private float alfa;

    void Start()
    {
        PlayerController pc = FindFirstObjectByType<PlayerController>();
        if (pc != null) jugador = pc.transform;

        contenido = new GameObject("Contenido").transform;
        contenido.SetParent(transform, false);
        posicionBase = contenido.localPosition;

        // Marco de madera (9-slice): el tamaño se da en unidades del sprite, compensando la escala
        GameObject goMarco = new GameObject("Marco");
        goMarco.transform.SetParent(contenido, false);
        goMarco.transform.localScale = new Vector3(EscalaMarco, EscalaMarco, 1f);
        marco = goMarco.AddComponent<SpriteRenderer>();
        marco.sprite = Resources.Load<Sprite>("UI/panel");
        marco.drawMode = SpriteDrawMode.Sliced;
        marco.size = new Vector2((tamano.x + 0.6f) / EscalaMarco, (tamano.y + 0.5f) / EscalaMarco);
        marco.color = new Color(1f, 1f, 1f, 0f);
        marco.sortingOrder = 14;

        // Texto (se ajusta solo al tamaño del cartel)
        GameObject goTexto = new GameObject("Texto");
        goTexto.transform.SetParent(contenido, false);
        texto = goTexto.AddComponent<TextMeshPro>();
        texto.font = Tipografia.Texto;
        texto.rectTransform.sizeDelta = tamano;
        texto.text = mensaje;
        texto.alignment = TextAlignmentOptions.Center;
        texto.enableAutoSizing = true;
        texto.fontSizeMin = 0.5f;
        texto.fontSizeMax = 4f;
        texto.lineSpacing = 4f;
        texto.color = new Color(1f, 0.95f, 0.84f, 0f);
        texto.outlineWidth = 0.15f;
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
        marco.color = new Color(1f, 1f, 1f, alfa);

        // Aparece creciendo un poco y flota suavemente mientras está visible
        float escala = Mathf.Lerp(0.85f, 1f, alfa);
        contenido.localScale = new Vector3(escala, escala, 1f);
        contenido.localPosition = posicionBase + Vector3.up * (Mathf.Sin(Time.time * 2f) * 0.06f * alfa);
    }
}
