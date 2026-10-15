using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// HUD de las escenas jugables: vidas (corazones), tamaño grande (envase de mermelada), puntos,
// tiempo y objetivo de la escena. Al entrar a la escena muestra un letrero con la ciudad,
// el lugar y una línea de la historia. Lo crea Partida al cargar cada escena de nivel.
public class HUDJuego : MonoBehaviour
{
    private const int CorazonesMaximos = 5;

    private static HUDJuego instancia;

    private PlayerHealth jugador;
    private Image[] corazones;
    private Image iconoGrande;
    private TextMeshProUGUI textoPuntos;
    private TextMeshProUGUI textoTiempo;

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
        // Franja superior semitransparente para que el HUD se lea sobre cualquier fondo
        RectTransform franja = InterfazUI.Crear("Franja", transform);
        franja.anchorMin = new Vector2(0f, 1f);
        franja.anchorMax = new Vector2(1f, 1f);
        franja.pivot = new Vector2(0.5f, 1f);
        franja.sizeDelta = new Vector2(0f, 130f);
        franja.anchoredPosition = Vector2.zero;
        franja.gameObject.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.35f);

        // Vidas
        Sprite corazon = InterfazUI.CargarSprite("UI/corazon");
        corazones = new Image[CorazonesMaximos];
        for (int i = 0; i < CorazonesMaximos; i++)
        {
            corazones[i] = InterfazUI.Imagen(transform, corazon, "Corazon" + i);
            InterfazUI.Colocar(corazones[i].rectTransform, new Vector2(0f, 1f), new Vector2(28 + i * 68, -22), new Vector2(60, 54));
        }

        // Tamaño grande (vida extra)
        iconoGrande = InterfazUI.Imagen(transform, InterfazUI.CargarSprite("UI/mermelada"), "Grande");
        InterfazUI.Colocar(iconoGrande.rectTransform, new Vector2(0f, 1f), new Vector2(28 + CorazonesMaximos * 68 + 12, -14), new Vector2(54, 72));

        // Puntos
        Image fresa = InterfazUI.Imagen(transform, InterfazUI.CargarSprite("UI/fresa"), "IconoPuntos");
        InterfazUI.Colocar(fresa.rectTransform, new Vector2(1f, 1f), new Vector2(-250, -16), new Vector2(54, 70));
        textoPuntos = InterfazUI.Texto(transform, "0", 52, InterfazUI.ColorTexto, TextAlignmentOptions.Left, "Puntos");
        InterfazUI.Colocar(textoPuntos.rectTransform, new Vector2(1f, 1f), new Vector2(-30, -18), new Vector2(210, 70));

        // Tiempo y objetivo
        textoTiempo = InterfazUI.Texto(transform, "Tiempo 00:00", 44, InterfazUI.ColorTexto, TextAlignmentOptions.Center, "Tiempo");
        InterfazUI.Colocar(textoTiempo.rectTransform, new Vector2(0.5f, 1f), new Vector2(0, -12), new Vector2(500, 60));
        TextMeshProUGUI objetivo = InterfazUI.Texto(transform, info.objetivo, 28, InterfazUI.ColorTitulo, TextAlignmentOptions.Center, "Objetivo");
        InterfazUI.Colocar(objetivo.rectTransform, new Vector2(0.5f, 1f), new Vector2(0, -74), new Vector2(1100, 44));

        StartCoroutine(Letrero(info));
    }

    void Update()
    {
        if (jugador == null) jugador = FindFirstObjectByType<PlayerHealth>();

        int vidas = jugador != null ? jugador.VidasActuales : 0;
        for (int i = 0; i < corazones.Length; i++) corazones[i].enabled = i < vidas;
        iconoGrande.enabled = jugador != null && jugador.EstaGrande;

        textoPuntos.text = Puntuacion.Total.ToString();
        textoTiempo.text = "Tiempo " + Partida.FormatoTiempo(Partida.Tiempo);
    }

    // Letrero de entrada: ciudad, lugar y una línea de la historia (narrativa del nivel)
    private IEnumerator Letrero(Partida.InfoEscena info)
    {
        RectTransform caja = InterfazUI.Colocar(InterfazUI.Crear("Letrero", transform), new Vector2(0.5f, 0.5f), new Vector2(0, 120), new Vector2(1400, 360));
        CanvasGroup grupo = caja.gameObject.AddComponent<CanvasGroup>();
        grupo.alpha = 0f;
        grupo.blocksRaycasts = false;
        caja.gameObject.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.5f);

        TextMeshProUGUI ciudad = InterfazUI.Texto(caja, info.ciudad.ToUpper(), 110, InterfazUI.ColorTitulo);
        InterfazUI.Colocar(ciudad.rectTransform, new Vector2(0.5f, 1f), new Vector2(0, -20), new Vector2(1300, 130));
        TextMeshProUGUI lugar = InterfazUI.Texto(caja, info.lugar, 56, InterfazUI.ColorTexto);
        InterfazUI.Colocar(lugar.rectTransform, new Vector2(0.5f, 1f), new Vector2(0, -150), new Vector2(1300, 70));
        TextMeshProUGUI historia = InterfazUI.Texto(caja, info.narrativa, 34, InterfazUI.ColorTexto);
        historia.fontStyle = FontStyles.Italic;
        InterfazUI.Colocar(historia.rectTransform, new Vector2(0.5f, 1f), new Vector2(0, -235), new Vector2(1300, 100));

        // Espera el fundido de la transición antes de aparecer
        yield return new WaitForSecondsRealtime(0.6f);
        yield return Fundir(grupo, 0f, 1f, 0.4f);
        yield return new WaitForSecondsRealtime(2.8f);
        yield return Fundir(grupo, 1f, 0f, 0.6f);
        Destroy(caja.gameObject);
    }

    private static IEnumerator Fundir(CanvasGroup grupo, float desde, float hasta, float duracion)
    {
        for (float t = 0f; t < duracion; t += Time.unscaledDeltaTime)
        {
            grupo.alpha = Mathf.Lerp(desde, hasta, t / duracion);
            yield return null;
        }
        grupo.alpha = hasta;
    }
}
