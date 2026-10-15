using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

// Ayudas para construir la interfaz por código (menús, HUD, ventanas).
// Todos los Canvas usan Canvas Scaler con resolución de referencia 1920x1080,
// así la interfaz se adapta a cualquier resolución y proporción de pantalla.
public static class InterfazUI
{
    public static readonly Color ColorPanel = new Color(0.08f, 0.06f, 0.12f, 0.88f);
    public static readonly Color ColorTexto = new Color(1f, 0.97f, 0.88f);
    public static readonly Color ColorTitulo = new Color(1f, 0.78f, 0.2f);
    public static readonly Color ColorTextoBoton = new Color(0.16f, 0.09f, 0.04f);

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

    // Rectángulo de color que llena a su padre (fondo de ventanas, velos)
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

    public static TextMeshProUGUI Texto(Transform padre, string texto, float tamano, Color color,
                                        TextAlignmentOptions alineacion = TextAlignmentOptions.Center, string nombre = "Texto")
    {
        RectTransform rt = Crear(nombre, padre);
        TextMeshProUGUI t = rt.gameObject.AddComponent<TextMeshProUGUI>();
        t.text = texto;
        t.fontSize = tamano;
        t.color = color;
        t.alignment = alineacion;
        t.raycastTarget = false;
        t.textWrappingMode = TextWrappingModes.Normal;
        // Contorno oscuro para que se lea sobre cualquier fondo
        t.outlineWidth = 0.2f;
        t.outlineColor = new Color32(27, 20, 32, 255);
        return t;
    }

    // Botón con la placa de madera del juego; responde al mouse, al teclado y al control
    public static Button Boton(Transform padre, string texto, UnityAction accion, float ancho = 520, float alto = 110)
    {
        RectTransform rt = Crear("Boton_" + texto, padre);
        rt.sizeDelta = new Vector2(ancho, alto);

        Image fondo = rt.gameObject.AddComponent<Image>();
        fondo.sprite = CargarSprite("UI/boton");

        Button boton = rt.gameObject.AddComponent<Button>();
        ColorBlock colores = boton.colors;
        colores.normalColor = Color.white;
        colores.highlightedColor = new Color(1f, 0.92f, 0.6f);
        colores.selectedColor = new Color(1f, 0.92f, 0.6f);
        colores.pressedColor = new Color(0.85f, 0.7f, 0.45f);
        colores.disabledColor = new Color(0.45f, 0.45f, 0.45f, 0.8f);
        colores.fadeDuration = 0.08f;
        boton.colors = colores;

        TextMeshProUGUI etiqueta = Texto(rt, texto, alto * 0.38f, ColorTextoBoton, TextAlignmentOptions.Center, "Etiqueta");
        etiqueta.outlineWidth = 0f;
        etiqueta.fontStyle = FontStyles.Bold;
        Estirar(etiqueta.rectTransform);

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

    // Deslizador 0..1 (volúmenes)
    public static Slider Deslizador(Transform padre, float valor, UnityAction<float> alCambiar)
    {
        RectTransform rt = Crear("Deslizador", padre);
        rt.sizeDelta = new Vector2(520, 40);
        Slider s = rt.gameObject.AddComponent<Slider>();

        Image fondo = Panel(rt, new Color(0.2f, 0.15f, 0.12f), "Fondo");
        fondo.rectTransform.offsetMin = new Vector2(0, 10);
        fondo.rectTransform.offsetMax = new Vector2(0, -10);

        RectTransform areaRelleno = Estirar(Crear("AreaRelleno", rt));
        areaRelleno.offsetMin = new Vector2(0, 10);
        areaRelleno.offsetMax = new Vector2(-20, -10);
        Image relleno = Panel(areaRelleno, new Color(0.95f, 0.65f, 0.15f), "Relleno");

        RectTransform areaManija = Estirar(Crear("AreaManija", rt));
        areaManija.offsetMin = new Vector2(10, 0);
        areaManija.offsetMax = new Vector2(-10, 0);
        RectTransform manija = Crear("Manija", areaManija);
        manija.sizeDelta = new Vector2(34, 0);
        Image imgManija = manija.gameObject.AddComponent<Image>();
        imgManija.color = new Color(1f, 0.95f, 0.85f);

        s.fillRect = relleno.rectTransform;
        s.handleRect = manija;
        s.targetGraphic = imgManija;
        s.minValue = 0f;
        s.maxValue = 1f;
        s.value = valor;
        s.onValueChanged.AddListener(alCambiar);
        return s;
    }

    // Ventana modal: velo oscuro que bloquea lo de atrás + caja centrada con título.
    // Devuelve la caja; para cerrarla se destruye su padre (el velo)
    public static RectTransform Ventana(Transform padre, string titulo, Vector2 tamano, Color colorTitulo)
    {
        Image velo = Panel(padre, new Color(0f, 0f, 0f, 0.6f), "Ventana_" + titulo);
        velo.raycastTarget = true;

        RectTransform caja = Colocar(Crear("Caja", velo.transform), new Vector2(0.5f, 0.5f), Vector2.zero, tamano);
        Image fondo = caja.gameObject.AddComponent<Image>();
        fondo.color = ColorPanel;
        Outline borde = caja.gameObject.AddComponent<Outline>();
        borde.effectColor = new Color(0.95f, 0.65f, 0.15f);
        borde.effectDistance = new Vector2(6, -6);

        TextMeshProUGUI t = Texto(caja, titulo, 88, colorTitulo, TextAlignmentOptions.Center, "Titulo");
        Colocar(t.rectTransform, new Vector2(0.5f, 1f), new Vector2(0, -30), new Vector2(tamano.x - 60, 110));
        return caja;
    }

    public static void CerrarVentana(RectTransform caja)
    {
        if (caja != null) Object.Destroy(caja.parent.gameObject);
    }

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
