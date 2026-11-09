using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// Controles táctiles para jugar en celular: joystick a la izquierda, botón de saltar a la derecha,
// botón de lanzar comida (solo en la escena del jefe) y botón de pausa.
// - Se muestran solos en un celular o tableta (Application.isMobilePlatform), o en cualquier pantalla
//   en cuanto se toca; si se vuelve a usar el teclado, se ocultan.
// - PlayerController, LanzadorComida y MenuPausa leen de aquí junto con el teclado y el control,
//   así que en computadora todo sigue funcionando igual.
// Lo crea Partida al cargar cada escena de nivel
public class ControlesMoviles : MonoBehaviour
{
    private const float ZonaMuerta = 0.3f;

    private static bool tocoPantalla;
    private static int cuadroDeteccion = -1;
    private static int cuadroSalto = -10;
    private static int cuadroLanzar = -10;
    private static int cuadroPausa = -10;

    private RectTransform raiz;
    private bool visible;

    // Verdadero si se está jugando con el dedo (celular, tableta o pantalla táctil tocada)
    public static bool Activos
    {
        get
        {
            Detectar();
            return Application.isMobilePlatform || tocoPantalla;
        }
    }

    // -1 (izquierda) a 1 (derecha), 0 si el joystick está quieto
    public static float Horizontal { get; private set; }

    public static bool ConsumirSalto() => Consumir(ref cuadroSalto);
    public static bool ConsumirLanzar() => Consumir(ref cuadroLanzar);
    public static bool ConsumirPausa() => Consumir(ref cuadroPausa);

    // Un toque vale solo por un par de cuadros: así no queda uno viejo guardado durante una pausa
    private static bool Consumir(ref int cuadro)
    {
        bool valido = Time.frameCount - cuadro <= 2;
        cuadro = -10;
        return valido;
    }

    private static void Detectar()
    {
        if (cuadroDeteccion == Time.frameCount) return;
        cuadroDeteccion = Time.frameCount;

        if (Touchscreen.current != null && Touchscreen.current.press.wasPressedThisFrame) tocoPantalla = true;
        else if (!Application.isMobilePlatform && Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame) tocoPantalla = false;
    }

    // En celular el mensaje del tutorial habla de los botones en pantalla en lugar de las teclas
    public static string AdaptarAyuda(string mensaje)
    {
        if (!Activos || !mensaje.Contains("A / D")) return mensaje;
        return $"Muévete con el <color={InterfazUI.Amarillo}>joystick</color>\nSalta con el botón <color={InterfazUI.Amarillo}>SALTA</color>";
    }

    public static void Crear()
    {
        Horizontal = 0f;
        cuadroSalto = cuadroLanzar = cuadroPausa = -10;

        Canvas canvas = InterfazUI.CrearCanvas("ControlesMoviles", 15);
        canvas.gameObject.AddComponent<ControlesMoviles>().Construir(canvas.transform);
    }

    private void Construir(Transform canvas)
    {
        raiz = InterfazUI.Estirar(InterfazUI.Crear("Controles", canvas));

        // Joystick (abajo a la izquierda)
        Palanca palanca = Palanca.Crear(raiz, new Vector2(0f, 0f), new Vector2(80, 80), 340f);

        // Saltar (abajo a la derecha)
        BotonTactil.Crear(raiz, "SALTA", new Vector2(1f, 0f), new Vector2(-80, 80), 240f, () => cuadroSalto = Time.frameCount, new Color(1f, 0.78f, 0.2f));

        // Lanzar comida: solo hay poder de lanzar en la escena del jefe
        if (FindFirstObjectByType<LanzadorComida>() != null)
        {
            BotonTactil.Crear(raiz, "LANZA", new Vector2(1f, 0f), new Vector2(-360, 230), 200f, () => cuadroLanzar = Time.frameCount, new Color(1f, 0.48f, 0.69f));
        }

        // Pausa (debajo de los puntos)
        BotonTactil.Crear(raiz, "II", new Vector2(1f, 1f), new Vector2(-30, -140), 100f, () => cuadroPausa = Time.frameCount, new Color(1f, 0.95f, 0.84f));

        raiz.gameObject.SetActive(false);
    }

    void Update()
    {
        bool ver = Activos && Time.timeScale > 0f && !Partida.Terminada;
        if (ver == visible) return;

        visible = ver;
        raiz.gameObject.SetActive(ver);
        if (!ver) Horizontal = 0f;
    }

    void OnDestroy()
    {
        Horizontal = 0f;
    }

    // ---------- Círculo suave generado por código (no se necesitan imágenes nuevas) ----------

    private static Sprite discoGrande;
    private static Sprite aro;

    private static Sprite Disco(bool soloAro)
    {
        if (soloAro && aro != null) return aro;
        if (!soloAro && discoGrande != null) return discoGrande;

        const int tam = 128;
        Texture2D tex = new Texture2D(tam, tam, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        tex.wrapMode = TextureWrapMode.Clamp;
        float c = (tam - 1) * 0.5f;
        for (int y = 0; y < tam; y++)
        {
            for (int x = 0; x < tam; x++)
            {
                float d = Vector2.Distance(new Vector2(x, y), new Vector2(c, c)) / c;
                float alfa = Mathf.Clamp01((1f - d) * c * 0.5f);
                // El aro conserva solo el borde (un 14% del radio)
                if (soloAro) alfa = Mathf.Min(alfa, Mathf.Clamp01((d - 0.84f) * c * 0.5f));
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, alfa));
            }
        }
        tex.Apply();
        Sprite s = Sprite.Create(tex, new Rect(0, 0, tam, tam), new Vector2(0.5f, 0.5f), 100f);
        if (soloAro) aro = s; else discoGrande = s;
        return s;
    }

    // ---------- Joystick ----------

    private class Palanca : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        private RectTransform zona;
        private RectTransform perilla;
        private float radio;

        public static Palanca Crear(Transform padre, Vector2 ancla, Vector2 posicion, float tamano)
        {
            RectTransform zona = InterfazUI.Colocar(InterfazUI.Crear("Joystick", padre), ancla, posicion, new Vector2(tamano, tamano));
            Image fondo = zona.gameObject.AddComponent<Image>();
            fondo.sprite = Disco(false);
            fondo.color = new Color(0.08f, 0.06f, 0.12f, 0.45f);

            Image borde = InterfazUI.Imagen(zona, Disco(true), "Aro");
            InterfazUI.Estirar(borde.rectTransform);
            borde.color = new Color(1f, 0.78f, 0.2f, 0.85f);

            Image bola = InterfazUI.Imagen(zona, Disco(false), "Perilla");
            RectTransform rt = bola.rectTransform;
            rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(tamano * 0.42f, tamano * 0.42f);
            bola.color = new Color(1f, 0.78f, 0.2f, 0.9f);

            Palanca p = zona.gameObject.AddComponent<Palanca>();
            p.zona = zona;
            p.perilla = rt;
            p.radio = tamano * 0.5f;
            return p;
        }

        public void OnPointerDown(PointerEventData e) => Mover(e);
        public void OnDrag(PointerEventData e) => Mover(e);
        public void OnPointerUp(PointerEventData e) => Soltar();
        void OnDisable() => Soltar();

        private void Mover(PointerEventData e)
        {
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(zona, e.position, e.pressEventCamera, out Vector2 local)) return;

            Vector2 v = Vector2.ClampMagnitude(local / radio, 1f);
            perilla.anchoredPosition = v * radio * 0.55f;
            Horizontal = Mathf.Abs(v.x) < ZonaMuerta ? 0f : Mathf.Sign(v.x);
        }

        private void Soltar()
        {
            Horizontal = 0f;
            if (perilla != null) perilla.anchoredPosition = Vector2.zero;
        }
    }

    // ---------- Botón redondo ----------

    private class BotonTactil : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        private Action alPresionar;
        private RectTransform rt;

        public static void Crear(Transform padre, string texto, Vector2 ancla, Vector2 posicion, float tamano, Action alPresionar, Color color)
        {
            RectTransform rt = InterfazUI.Colocar(InterfazUI.Crear("Boton" + texto, padre), ancla, posicion, new Vector2(tamano, tamano));
            Image fondo = rt.gameObject.AddComponent<Image>();
            fondo.sprite = Disco(false);
            fondo.color = new Color(0.08f, 0.06f, 0.12f, 0.55f);

            Image borde = InterfazUI.Imagen(rt, Disco(true), "Aro");
            InterfazUI.Estirar(borde.rectTransform);
            borde.color = color;

            TextMeshProUGUI t = InterfazUI.Texto(rt, texto, tamano * 0.2f, color, TextAlignmentOptions.Center, "Texto", true);
            InterfazUI.Estirar(t.rectTransform);
            t.raycastTarget = false;

            BotonTactil b = rt.gameObject.AddComponent<BotonTactil>();
            b.alPresionar = alPresionar;
            b.rt = rt;
        }

        public void OnPointerDown(PointerEventData e)
        {
            alPresionar?.Invoke();
            rt.localScale = Vector3.one * 0.9f;
        }

        public void OnPointerUp(PointerEventData e) => rt.localScale = Vector3.one;
        void OnDisable() => rt.localScale = Vector3.one;
    }
}
