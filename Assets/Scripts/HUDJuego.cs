using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// HUD de las escenas jugables, con marcos de madera que se ajustan solos a su contenido:
// - Izquierda: vidas (corazones) y el envase de mermelada cuando el Estudiante está grande.
// - Centro: tiempo y, debajo, el objetivo de la zona.
// - Derecha: puntos con el ícono del coleccionable del nivel (fresa, charamusca...).
// - Abajo al centro: el recuadro de ayudas del tutorial (AyudaTutorial).
// Al entrar a la zona baja un letrero con papel picado, la ciudad, el lugar y una línea de la historia.
// Lo crea Partida al cargar cada escena de nivel
public class HUDJuego : MonoBehaviour
{
    private const int CorazonesMaximos = 5;
    private const float AltoMarco = 92f;

    private static HUDJuego instancia;

    // Verdadero mientras el letrero de entrada (ciudad, lugar y línea de historia) sigue en pantalla
    public static bool LetreroActivo { get; private set; }

    private PlayerHealth jugador;
    private GameObject[] corazones;
    private GameObject iconoGrande;
    private TextMeshProUGUI textoPuntos;
    private TextMeshProUGUI textoTiempo;
    private RectTransform marcoPuntos;
    private Image iconoLlave;
    private RectTransform marcoJefe;
    private Image[] comidasJefe;
    private GameObject avisoActual;
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
        LetreroActivo = false;
    }

    // Barra del jefe (abajo al centro): su nombre, la tecla para lanzar comida y una fresa por cada bocado que ya se comió.
    // La fresa se enciende cada vez que el jefe se come algo
    public static void Jefe(string nombre, int comidas, int total)
    {
        if (instancia != null) instancia.PonerJefe(nombre, comidas, total);
    }

    public static void OcultarJefe()
    {
        if (instancia != null && instancia.marcoJefe != null) Destroy(instancia.marcoJefe.gameObject);
    }

    private void PonerJefe(string nombre, int comidas, int total)
    {
        if (marcoJefe == null)
        {
            Image placa = InterfazUI.Placa(transform, "MarcoJefe", 3.2f);
            marcoJefe = InterfazUI.Colocar(placa.rectTransform, new Vector2(0.5f, 0f), new Vector2(0, 28), new Vector2(760, 150));

            TextMeshProUGUI titulo = InterfazUI.Texto(placa.transform, nombre.ToUpper() + "   ·   LÁNZALE COMIDA CON  F", 26, InterfazUI.ColorTitulo, TextAlignmentOptions.Center, "TituloJefe");
            InterfazUI.Colocar(titulo.rectTransform, new Vector2(0.5f, 1f), new Vector2(0, -34), new Vector2(720, 40));
            InterfazUI.Sombra(titulo, 0.8f);

            Sprite fresa = InterfazUI.CargarSprite("UI/fresa");
            comidasJefe = new Image[total];
            for (int i = 0; i < total; i++)
            {
                Image icono = InterfazUI.Imagen(placa.transform, fresa, "Bocado" + (i + 1));
                InterfazUI.Colocar(icono.rectTransform, new Vector2(0.5f, 0f), new Vector2((i - (total - 1) * 0.5f) * 72f, 52f), new Vector2(42, 54));
                comidasJefe[i] = icono;
            }
        }

        for (int i = 0; i < comidasJefe.Length; i++)
        {
            comidasJefe[i].color = i < comidas ? Color.white : new Color(0.15f, 0.12f, 0.14f, 0.55f);
        }
    }

    // Mensaje breve en el centro de la pantalla (ej. "Necesitas la llave de la Parroquia"); cualquier script puede pedirlo
    public static void Aviso(string texto)
    {
        if (instancia != null) instancia.StartCoroutine(instancia.MostrarAviso(texto));
    }

    private IEnumerator MostrarAviso(string texto)
    {
        if (avisoActual != null) Destroy(avisoActual);

        TextMeshProUGUI t = InterfazUI.Texto(transform, texto, 44, InterfazUI.ColorTitulo, TextAlignmentOptions.Center, "Aviso");
        InterfazUI.Colocar(t.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0, 180), new Vector2(1500, 80));
        InterfazUI.Sombra(t, 1f);
        avisoActual = t.gameObject;

        yield return new WaitForSecondsRealtime(2f);
        for (float k = 0f; k < 0.5f && t != null; k += Time.unscaledDeltaTime)
        {
            Color c = t.color;
            c.a = 1f - k / 0.5f;
            t.color = c;
            yield return null;
        }
        if (t != null) Destroy(t.gameObject);
    }

    private void Construir(Partida.InfoEscena info)
    {
        // Vidas
        RectTransform vidas = Marco("Vidas", new Vector2(0f, 1f), new Vector2(20, -18));
        Sprite corazon = InterfazUI.CargarSprite("UI/corazon");
        corazones = new GameObject[CorazonesMaximos];
        for (int i = 0; i < CorazonesMaximos; i++) corazones[i] = Icono(vidas, corazon, 44, 40).gameObject;
        iconoGrande = Icono(vidas, InterfazUI.CargarSprite("UI/mermelada"), 36, 48).gameObject;

        // Tiempo
        RectTransform tiempo = Marco("Tiempo", new Vector2(0.5f, 1f), new Vector2(0, -18));
        textoTiempo = Numero(tiempo, "00:00", 30, TextAlignmentOptions.Midline, 150);

        // Objetivo de la zona (texto con sombra, sin marco para no saturar)
        TextMeshProUGUI objetivo = InterfazUI.Texto(transform, info.objetivo, 28, InterfazUI.ColorTitulo, TextAlignmentOptions.Center, "Objetivo");
        InterfazUI.Colocar(objetivo.rectTransform, new Vector2(0.5f, 1f), new Vector2(0, -116), new Vector2(1100, 40));
        InterfazUI.Sombra(objetivo, 0.8f);

        // Puntos
        marcoPuntos = Marco("Puntos", new Vector2(1f, 1f), new Vector2(-20, -18));
        // La llave del nivel (HU15) aparece a la izquierda de los puntos en cuanto se recoge
        iconoLlave = Icono(marcoPuntos, InterfazUI.CargarSprite("UI/llave"), 36, 48);
        iconoLlave.gameObject.SetActive(Llave.Tiene);
        Icono(marcoPuntos, InterfazUI.CargarSprite(info.iconoColeccionable), 36, 48);
        textoPuntos = Numero(marcoPuntos, "0", 30, TextAlignmentOptions.MidlineRight, 120);

        AyudaTutorial.Crear(transform);
        StartCoroutine(Letrero(info));
    }

    // Marco de madera que crece a lo ancho según lo que contiene (todo queda dentro del borde)
    private RectTransform Marco(string nombre, Vector2 ancla, Vector2 posicion)
    {
        Image placa = InterfazUI.Placa(transform, "Marco" + nombre, 3.2f);
        RectTransform rt = InterfazUI.Colocar(placa.rectTransform, ancla, posicion, new Vector2(100, AltoMarco));

        HorizontalLayoutGroup h = placa.gameObject.AddComponent<HorizontalLayoutGroup>();
        h.padding = new RectOffset(30, 30, 22, 22);
        h.spacing = 12;
        h.childAlignment = TextAnchor.MiddleCenter;
        h.childControlWidth = true;
        h.childControlHeight = true;
        h.childForceExpandWidth = false;
        h.childForceExpandHeight = false;

        ContentSizeFitter ajuste = placa.gameObject.AddComponent<ContentSizeFitter>();
        ajuste.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        return rt;
    }

    private static Image Icono(Transform padre, Sprite sprite, float ancho, float alto)
    {
        Image img = InterfazUI.Imagen(padre, sprite, "Icono");
        LayoutElement le = img.gameObject.AddComponent<LayoutElement>();
        le.preferredWidth = ancho;
        le.preferredHeight = alto;
        return img;
    }

    // Número en tipografía de título, centrado por su línea media para que quede justo al centro del marco
    private static TextMeshProUGUI Numero(Transform padre, string texto, float tamano, TextAlignmentOptions alineacion, float anchoMinimo)
    {
        TextMeshProUGUI t = InterfazUI.Texto(padre, texto, tamano, InterfazUI.ColorTexto, alineacion, "Numero", true);
        t.textWrappingMode = TextWrappingModes.NoWrap;
        LayoutElement le = t.gameObject.AddComponent<LayoutElement>();
        le.minWidth = anchoMinimo;
        le.preferredHeight = AltoMarco - 44f;
        return t;
    }

    void Update()
    {
        if (jugador == null) jugador = FindFirstObjectByType<PlayerHealth>();

        int vidas = jugador != null ? jugador.VidasActuales : 0;
        for (int i = 0; i < corazones.Length; i++)
        {
            bool visible = i < vidas;
            if (corazones[i].activeSelf != visible) corazones[i].SetActive(visible);
        }
        bool grande = jugador != null && jugador.EstaGrande;
        if (iconoGrande.activeSelf != grande) iconoGrande.SetActive(grande);

        if (iconoLlave != null && iconoLlave.gameObject.activeSelf != Llave.Tiene) iconoLlave.gameObject.SetActive(Llave.Tiene);

        textoTiempo.text = Partida.FormatoTiempo(Partida.Tiempo);

        // Los puntos dan un pequeño salto cada vez que suben
        if (Puntuacion.Total != puntosMostrados)
        {
            if (puntosMostrados >= 0) rebotePuntos = 1f;
            puntosMostrados = Puntuacion.Total;
            textoPuntos.text = puntosMostrados.ToString();
        }
        rebotePuntos = Mathf.MoveTowards(rebotePuntos, 0f, Time.unscaledDeltaTime * 4f);
        float e = 1f + 0.1f * rebotePuntos;
        marcoPuntos.localScale = new Vector3(e, e, 1f);
    }

    // Letrero de entrada: papel picado, ciudad, lugar en el listón y una línea de la historia.
    // Baja desde arriba con un pequeño rebote, se queda unos segundos y vuelve a subir
    private IEnumerator Letrero(Partida.InfoEscena info)
    {
        LetreroActivo = true;
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
        LetreroActivo = false;
    }
}
