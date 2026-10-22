using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

// Ayudas para construir la interfaz por código con el estilo del juego:
// marcos de madera, listón rojo para títulos, papel picado, botones de madera y tipografías pixel.
// Todos los Canvas usan Canvas Scaler con resolución de referencia 1920x1080,
// así la interfaz se adapta a cualquier resolución y proporción de pantalla.
public static class InterfazUI
{
    public static readonly Color ColorPanel = new Color(0.08f, 0.06f, 0.12f, 0.88f);
    public static readonly Color ColorTexto = new Color(1f, 0.95f, 0.84f);
    public static readonly Color ColorTitulo = new Color(1f, 0.78f, 0.2f);
    public static readonly Color ColorTextoBoton = new Color(0.24f, 0.12f, 0.04f);
    public static readonly Color ColorContorno = new Color32(27, 20, 32, 255);

    // Hex para usar dentro del texto enriquecido (<color=...>)
    public const string Amarillo = "#FFC832";
    public const string Rosa = "#FF7BB0";

    public static Canvas CrearCanvas(string nombre, int orden, bool persistente = false)
    {
        GameObject go = new GameObject(nombre);
        if (persistente) Object.DontDestroyOnLoad(go);

        Canvas canvas = go.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = orden;

        CanvasScaler escalador = go.AddComponent<CanvasScaler>();
        escalador.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        escalador.referenceResolution = new Vector2(1920, 1080);
        escalador.matchWidthOrHeight = 0.5f;

        go.AddComponent<GraphicRaycaster>();
        AsegurarEventSystem();
        return canvas;
    }

    // Sin EventSystem los botones no responden al mouse, al teclado ni al control
    public static void AsegurarEventSystem()
    {
        if (Object.FindFirstObjectByType<EventSystem>() != null) return;

        GameObject es = new GameObject("EventSystem");
        es.AddComponent<EventSystem>();
        es.AddComponent<InputSystemUIInputModule>();
    }

    // ---------- Posición ----------

    public static RectTransform Crear(string nombre, Transform padre)
    {
        GameObject go = new GameObject(nombre, typeof(RectTransform));
        go.transform.SetParent(padre, false);
        return (RectTransform)go.transform;
    }

    // Coloca un elemento: ancla (0..1), posición relativa al ancla y tamaño en píxeles de referencia
    public static RectTransform Colocar(RectTransform rt, Vector2 ancla, Vector2 posicion, Vector2 tamano)
    {
        rt.anchorMin = ancla;
        rt.anchorMax = ancla;
        rt.pivot = ancla;
        rt.anchoredPosition = posicion;
        rt.sizeDelta = tamano;
        return rt;
    }

    public static RectTransform Estirar(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        return rt;
    }

    // ---------- Piezas visuales ----------

    // Rectángulo de color que llena a su padre (velos, oscurecer el fondo)
    public static Image Panel(Transform padre, Color color, string nombre = "Panel")
    {
        RectTransform rt = Estirar(Crear(nombre, padre));
        Image img = rt.gameObject.AddComponent<Image>();
        img.color = color;
        return img;
    }

    public static Image Imagen(Transform padre, Sprite sprite, string nombre = "Imagen")
    {
        RectTransform rt = Crear(nombre, padre);
        Image img = rt.gameObject.AddComponent<Image>();
        img.sprite = sprite;
        img.preserveAspect = true;
        img.raycastTarget = false;
        return img;
    }

    // Marco de madera que se estira sin deformar las esquinas (9-slice)
    public static Image Placa(Transform padre, string nombre = "Placa", float grosorBorde = 1.8f)
    {
        Image img = Crear(nombre, padre).gameObject.AddComponent<Image>();
        img.sprite = CargarSprite("UI/panel");
        img.type = Image.Type.Sliced;
        img.pixelsPerUnitMultiplier = grosorBorde;
        img.raycastTarget = false;
        return img;
    }

    // Listón rojo con texto en tipografía de título
    public static RectTransform Liston(Transform padre, string texto, float ancho, float alto = 110f, float tamanoTexto = 34f)
    {
        RectTransform rt = Crear("Liston", padre);
        rt.sizeDelta = new Vector2(ancho, alto);
        Image img = rt.gameObject.AddComponent<Image>();
        img.sprite = CargarSprite("UI/liston");
        img.type = Image.Type.Sliced;
        img.pixelsPerUnitMultiplier = 255f / alto;
        img.raycastTarget = false;

        TextMeshProUGUI t = Texto(rt, texto, tamanoTexto, Color.white, TextAlignmentOptions.Center, "Texto", true);
        Estirar(t.rectTransform);
        t.rectTransform.offsetMin = new Vector2(ancho * 0.18f, alto * 0.12f);
        t.rectTransform.offsetMax = new Vector2(-ancho * 0.18f, 0f);
        t.textWrappingMode = TextWrappingModes.NoWrap;
        t.enableAutoSizing = true;
        t.fontSizeMin = 12;
        t.fontSizeMax = tamanoTexto;
        return rt;
    }

    // Tira de papel picado que se repite a lo ancho
    public static Image PapelPicado(Transform padre, float alto = 120f)
    {
        Image img = Crear("PapelPicado", padre).gameObject.AddComponent<Image>();
        img.sprite = CargarSprite("UI/papel_picado");
        img.type = Image.Type.Tiled;
        img.pixelsPerUnitMultiplier = 218f / alto;
        img.raycastTarget = false;
        return img;
    }

    // ---------- Texto ----------

    // Texto con contorno oscuro. "titulo" usa la tipografía arcade (Press Start 2P) con sombra
    public static TextMeshProUGUI Texto(Transform padre, string texto, float tamano, Color color,
                                        TextAlignmentOptions alineacion = TextAlignmentOptions.Center,
                                        string nombre = "Texto", bool titulo = false)
    {
        RectTransform rt = Crear(nombre, padre);
        TextMeshProUGUI t = rt.gameObject.AddComponent<TextMeshProUGUI>();
        t.font = titulo ? Tipografia.Titulo : Tipografia.Texto;
        t.text = texto;
        t.fontSize = tamano;
        t.color = color;
        t.alignment = alineacion;
        t.raycastTarget = false;
        t.textWrappingMode = TextWrappingModes.Normal;
        t.lineSpacing = titulo ? 20f : 6f;
        t.outlineWidth = titulo ? 0.12f : 0.18f;
        t.outlineColor = ColorContorno;
        if (titulo) Sombra(t);
        return t;
    }

    // Sombra dura hacia abajo a la derecha, como en los juegos de 8 bits
    public static void Sombra(TMP_Text t, float distancia = 1.2f)
    {
        Material m = t.fontMaterial;
        m.EnableKeyword(ShaderUtilities.Keyword_Underlay);
        m.SetColor(ShaderUtilities.ID_UnderlayColor, ColorContorno);
        m.SetFloat(ShaderUtilities.ID_UnderlayOffsetX, distancia);
        m.SetFloat(ShaderUtilities.ID_UnderlayOffsetY, -distancia);
        m.SetFloat(ShaderUtilities.ID_UnderlayDilate, 0.5f);
    }

    // ---------- Controles ----------

    // Botón de madera; al seleccionarlo (mouse, teclado o control) crece y aparece una fresa a su lado
    public static Button Boton(Transform padre, string texto, UnityAction accion, float ancho = 520, float alto = 110)
    {
        RectTransform rt = Crear("Boton_" + texto, padre);
        rt.sizeDelta = new Vector2(ancho, alto);

        Image fondo = rt.gameObject.AddComponent<Image>();
        fondo.sprite = CargarSprite("UI/boton");

        Button boton = rt.gameObject.AddComponent<Button>();
        ColorBlock colores = boton.colors;
        colores.normalColor = Color.white;
        colores.highlightedColor = new Color(1f, 0.96f, 0.75f);
        colores.selectedColor = new Color(1f, 0.96f, 0.75f);
        colores.pressedColor = new Color(0.85f, 0.7f, 0.45f);
        colores.disabledColor = new Color(0.5f, 0.45f, 0.42f, 0.85f);
        colores.fadeDuration = 0.08f;
        boton.colors = colores;

        TextMeshProUGUI etiqueta = Texto(rt, texto, alto * 0.36f, ColorTextoBoton, TextAlignmentOptions.Center, "Etiqueta");
        etiqueta.outlineWidth = 0f;
        etiqueta.fontStyle = FontStyles.Bold;
        Estirar(etiqueta.rectTransform);
        etiqueta.rectTransform.offsetMin = new Vector2(ancho * 0.08f, alto * 0.08f);
        etiqueta.rectTransform.offsetMax = new Vector2(-ancho * 0.08f, 0f);

        rt.gameObject.AddComponent<BotonAnimado>();

        boton.onClick.AddListener(() => Sonido.Efecto("boton"));
        if (accion != null) boton.onClick.AddListener(accion);
        return boton;
    }

    // Columna de botones centrada (los ordena de arriba hacia abajo)
    public static RectTransform Columna(Transform padre, Vector2 ancla, Vector2 posicion, float espacio = 18f)
    {
        RectTransform rt = Colocar(Crear("Columna", padre), ancla, posicion, new Vector2(600, 800));
        VerticalLayoutGroup v = rt.gameObject.AddComponent<VerticalLayoutGroup>();
        v.spacing = espacio;
        v.childAlignment = TextAnchor.UpperCenter;
        v.childControlWidth = false;
        v.childControlHeight = false;
        v.childForceExpandWidth = false;
        v.childForceExpandHeight = false;
        return rt;
    }

    // Deslizador 0..1 (volúmenes) con una fresa como manija
    public static Slider Deslizador(Transform padre, float valor, UnityAction<float> alCambiar)
    {
        RectTransform rt = Crear("Deslizador", padre);
        rt.sizeDelta = new Vector2(520, 50);
        Slider s = rt.gameObject.AddComponent<Slider>();

        Image fondo = Panel(rt, new Color(0.13f, 0.08f, 0.05f), "Fondo");
        fondo.rectTransform.offsetMin = new Vector2(0, 14);
        fondo.rectTransform.offsetMax = new Vector2(0, -14);
        Outline borde = fondo.gameObject.AddComponent<Outline>();
        borde.effectColor = new Color(0.91f, 0.64f, 0.23f);
        borde.effectDistance = new Vector2(3, -3);

        RectTransform areaRelleno = Estirar(Crear("AreaRelleno", rt));
        areaRelleno.offsetMin = new Vector2(0, 14);
        areaRelleno.offsetMax = new Vector2(-24, -14);
        Image relleno = Panel(areaRelleno, new Color(0.84f, 0.16f, 0.22f), "Relleno");

        RectTransform areaManija = Estirar(Crear("AreaManija", rt));
        areaManija.offsetMin = new Vector2(12, 0);
        areaManija.offsetMax = new Vector2(-12, 0);
        RectTransform manija = Crear("Manija", areaManija);
        manija.sizeDelta = new Vector2(44, 10);
        Image imgManija = manija.gameObject.AddComponent<Image>();
        imgManija.sprite = CargarSprite("UI/fresa");
        imgManija.preserveAspect = true;

        s.fillRect = relleno.rectTransform;
        s.handleRect = manija;
        s.targetGraphic = imgManija;
        s.minValue = 0f;
        s.maxValue = 1f;
        s.value = valor;
        s.onValueChanged.AddListener(alCambiar);
        return s;
    }

    // ---------- Ventanas ----------

    // Ventana modal: velo oscuro que bloquea lo de atrás + marco de madera con el título en un listón.
    // Devuelve la caja; para cerrarla se destruye su padre (el velo)
    public static RectTransform Ventana(Transform padre, string titulo, Vector2 tamano, Color colorTitulo)
    {
        Image velo = Panel(padre, new Color(0.03f, 0.02f, 0.06f, 0.7f), "Ventana_" + titulo);
        velo.raycastTarget = true;

        Image placa = Placa(velo.transform, "Caja");
        RectTransform caja = Colocar(placa.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0, -20), tamano);

        RectTransform liston = Liston(caja, titulo, Mathf.Min(tamano.x * 0.8f, 900f), 120f, 40f);
        Colocar(liston, new Vector2(0.5f, 1f), new Vector2(0, 55), liston.sizeDelta);
        // El listón es rojo: el título va en blanco; el color pedido se usa como acento en el borde del velo
        Outline acento = placa.gameObject.AddComponent<Outline>();
        acento.effectColor = new Color(colorTitulo.r, colorTitulo.g, colorTitulo.b, 0.35f);
        acento.effectDistance = new Vector2(8, -8);
        return caja;
    }

    public static void CerrarVentana(RectTransform caja)
    {
        if (caja != null) Object.Destroy(caja.parent.gameObject);
    }

    // ---------- Utilidades ----------

    public static Sprite CargarSprite(string ruta)
    {
        Sprite s = Resources.Load<Sprite>(ruta);
        if (s == null) Debug.LogWarning("InterfazUI: no existe el sprite Resources/" + ruta);
        return s;
    }

    // Deja seleccionado un botón para poder navegar con teclado o control
    public static void Seleccionar(Selectable s)
    {
        if (s == null || EventSystem.current == null) return;
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(s.gameObject);
    }
}
