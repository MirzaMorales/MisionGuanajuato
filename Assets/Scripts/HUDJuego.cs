using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// HUD de las escenas jugables, con marcos de madera:
// - Izquierda: vidas (corazones) y el envase de mermelada cuando el Estudiante está grande.
// - Centro: tiempo y, debajo, el objetivo de la zona.
// - Derecha: puntos con el ícono de fresa.
// Al entrar a la zona baja un letrero con papel picado, la ciudad, el lugar y una línea de la historia.
// Lo crea Partida al cargar cada escena de nivel
public class HUDJuego : MonoBehaviour
{
    private const int CorazonesMaximos = 5;

    private static HUDJuego instancia;

    private PlayerHealth jugador;
    private Image[] corazones;
    private Image iconoGrande;
    private TextMeshProUGUI textoPuntos;
    private TextMeshProUGUI textoTiempo;
    private RectTransform marcoPuntos;
    private RectTransform marcoVidas;
    private int puntosMostrados = -1;
    private float rebotePuntos;

    // PlayerHealth oculta su texto de depuración de vidas cuando el HUD está activo
    public static bool Activo => instancia != null;

    public static void Crear(Partida.InfoEscena info)
    {
        Canvas canvas = InterfazUI.CrearCanvas("HUD", 10);
        instancia = canvas.gameObject.AddComponent<HUDJuego>();
        instancia.Construir(info);
    }

    void OnDestroy()
    {
        if (instancia == this) instancia = null;
    }

    private void Construir(Partida.InfoEscena info)
    {
        // Vidas
        Image placaVidas = InterfazUI.Placa(transform, "MarcoVidas", 2.6f);
        marcoVidas = InterfazUI.Colocar(placaVidas.rectTransform, new Vector2(0f, 1f), new Vector2(20, -18), new Vector2(470, 104));
        Sprite corazon = InterfazUI.CargarSprite("UI/corazon");
        corazones = new Image[CorazonesMaximos];
        for (int i = 0; i < CorazonesMaximos; i++)
        {
            corazones[i] = InterfazUI.Imagen(placaVidas.transform, corazon, "Corazon" + i);
            InterfazUI.Colocar(corazones[i].rectTransform, new Vector2(0f, 0.5f), new Vector2(34 + i * 64, 0), new Vector2(56, 50));
        }
        iconoGrande = InterfazUI.Imagen(placaVidas.transform, InterfazUI.CargarSprite("UI/mermelada"), "Grande");
        InterfazUI.Colocar(iconoGrande.rectTransform, new Vector2(0f, 0.5f), new Vector2(34 + CorazonesMaximos * 64 + 6, 0), new Vector2(46, 62));

        // Tiempo
        Image marcoTiempo = InterfazUI.Placa(transform, "MarcoTiempo", 2.6f);
        InterfazUI.Colocar(marcoTiempo.rectTransform, new Vector2(0.5f, 1f), new Vector2(0, -18), new Vector2(330, 84));
        textoTiempo = InterfazUI.Texto(marcoTiempo.transform, "00:00", 34, InterfazUI.ColorTexto, TextAlignmentOptions.Center, "Tiempo", true);
        InterfazUI.Estirar(textoTiempo.rectTransform);
        textoTiempo.rectTransform.offsetMin = new Vector2(20, 8);

        // Objetivo de la zona (texto con sombra, sin marco para no saturar)
        TextMeshProUGUI objetivo = InterfazUI.Texto(transform, info.objetivo, 30, InterfazUI.ColorTitulo, TextAlignmentOptions.Center, "Objetivo");
        InterfazUI.Colocar(objetivo.rectTransform, new Vector2(0.5f, 1f), new Vector2(0, -108), new Vector2(1100, 44));
        InterfazUI.Sombra(objetivo, 0.8f);

        // Puntos
        Image marco = InterfazUI.Placa(transform, "MarcoPuntos", 2.6f);
        marcoPuntos = InterfazUI.Colocar(marco.rectTransform, new Vector2(1f, 1f), new Vector2(-20, -18), new Vector2(330, 104));
        Image fresa = InterfazUI.Imagen(marco.transform, InterfazUI.CargarSprite("UI/fresa"), "IconoPuntos");
        InterfazUI.Colocar(fresa.rectTransform, new Vector2(0f, 0.5f), new Vector2(28, 2), new Vector2(52, 66));
        textoPuntos = InterfazUI.Texto(marco.transform, "0", 40, InterfazUI.ColorTexto, TextAlignmentOptions.Right, "Puntos", true);
        InterfazUI.Colocar(textoPuntos.rectTransform, new Vector2(1f, 0.5f), new Vector2(-30, -2), new Vector2(220, 60));

        StartCoroutine(Letrero(info));
    }

    void Update()
    {
        if (jugador == null) jugador = FindFirstObjectByType<PlayerHealth>();

        int vidas = jugador != null ? jugador.VidasActuales : 0;
        for (int i = 0; i < corazones.Length; i++) corazones[i].enabled = i < vidas;
        iconoGrande.enabled = jugador != null && jugador.EstaGrande;

        // El marco crece o se encoge según los corazones (y el envase) que muestra
        int espacios = Mathf.Max(1, Mathf.Min(vidas, CorazonesMaximos));
        float ancho = 40 + espacios * 64 + 10;
        iconoGrande.rectTransform.anchoredPosition = new Vector2(34 + espacios * 64 + 6, 0);
        if (iconoGrande.enabled) ancho += 60;
        marcoVidas.sizeDelta = new Vector2(ancho, marcoVidas.sizeDelta.y);
        textoTiempo.text = Partida.FormatoTiempo(Partida.Tiempo);

        // Los puntos dan un pequeño salto cada vez que suben
        if (Puntuacion.Total != puntosMostrados)
        {
            if (puntosMostrados >= 0) rebotePuntos = 1f;
            puntosMostrados = Puntuacion.Total;
            textoPuntos.text = puntosMostrados.ToString();
        }
        rebotePuntos = Mathf.MoveTowards(rebotePuntos, 0f, Time.unscaledDeltaTime * 4f);
        float e = 1f + 0.12f * rebotePuntos;
        marcoPuntos.localScale = new Vector3(e, e, 1f);
    }

    // Letrero de entrada: papel picado, ciudad, lugar en el listón y una línea de la historia.
    // Baja desde arriba con un pequeño rebote, se queda unos segundos y vuelve a subir
    private IEnumerator Letrero(Partida.InfoEscena info)
    {
        RectTransform grupo = InterfazUI.Colocar(InterfazUI.Crear("Letrero", transform), new Vector2(0.5f, 1f), Vector2.zero, new Vector2(1500, 560));
        CanvasGroup alfa = grupo.gameObject.AddComponent<CanvasGroup>();
        alfa.blocksRaycasts = false;

        Image papel = InterfazUI.PapelPicado(grupo, 110f);
        InterfazUI.Colocar(papel.rectTransform, new Vector2(0.5f, 1f), new Vector2(0, 0), new Vector2(1500, 110));

        TextMeshProUGUI ciudad = InterfazUI.Texto(grupo, info.ciudad.ToUpper(), 84, InterfazUI.ColorTitulo, TextAlignmentOptions.Center, "Ciudad", true);
        InterfazUI.Colocar(ciudad.rectTransform, new Vector2(0.5f, 1f), new Vector2(0, -150), new Vector2(1400, 110));
        InterfazUI.Sombra(ciudad, 2f);

        RectTransform lugar = InterfazUI.Liston(grupo, info.lugar, 900, 110, 36);
        InterfazUI.Colocar(lugar, new Vector2(0.5f, 1f), new Vector2(0, -275), lugar.sizeDelta);

        Image placa = InterfazUI.Placa(grupo, "Historia", 2.6f);
        InterfazUI.Colocar(placa.rectTransform, new Vector2(0.5f, 1f), new Vector2(0, -400), new Vector2(1120, 110));
        TextMeshProUGUI historia = InterfazUI.Texto(placa.transform, info.narrativa, 32, InterfazUI.ColorTexto, TextAlignmentOptions.Center, "Linea");
        historia.fontStyle = FontStyles.Italic;
        InterfazUI.Estirar(historia.rectTransform);
        historia.rectTransform.offsetMin = new Vector2(40, 10);
        historia.rectTransform.offsetMax = new Vector2(-40, -10);

        // Espera el fundido de la transición antes de aparecer
        alfa.alpha = 0f;
        yield return new WaitForSecondsRealtime(0.5f);

        const float arriba = 560f, abajo = -190f;
        for (float t = 0f; t < 0.6f; t += Time.unscaledDeltaTime)
        {
            float k = t / 0.6f;
            // Rebote suave al llegar (ease out back)
            float c = 1.6f;
            float r = 1f + (c + 1f) * Mathf.Pow(k - 1f, 3) + c * Mathf.Pow(k - 1f, 2);
            grupo.anchoredPosition = new Vector2(0, Mathf.LerpUnclamped(arriba, abajo, r));
            alfa.alpha = Mathf.Clamp01(k * 2f);
            yield return null;
        }
        grupo.anchoredPosition = new Vector2(0, abajo);
        alfa.alpha = 1f;

        yield return new WaitForSecondsRealtime(2.8f);

        for (float t = 0f; t < 0.5f; t += Time.unscaledDeltaTime)
        {
            float k = t / 0.5f;
            grupo.anchoredPosition = new Vector2(0, Mathf.Lerp(abajo, arriba, k * k));
            alfa.alpha = 1f - k;
            yield return null;
        }
        Destroy(grupo.gameObject);
    }
}
