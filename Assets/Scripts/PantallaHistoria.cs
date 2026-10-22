using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// Introducción de la historia antes de empezar una partida nueva. El texto aparece letra por letra
// dentro de un marco de madera, con el papel picado arriba y el Estudiante a un lado.
// - Enter, Espacio, clic o A: primero muestran todo el texto y después comienzan el juego.
// - "Omitir introducción", Esc o B: comienzan el juego de inmediato.
// Después de verla una vez, el botón "Omitir introducción" queda seleccionado (basta con Enter)
public class PantallaHistoria : MonoBehaviour
{
    private const float LetrasPorSegundo = 45f;
    private const string ClaveVista = "IntroduccionVista";

    private static readonly string[] Parrafos =
    {
        "Un día, tu novia recibió una <color=" + InterfazUI.Amarillo + ">misteriosa invitación</color> para visitar distintos lugares de Guanajuato... y desapareció.",
        "Descubres que está atrapada en la <color=" + InterfazUI.Amarillo + ">Catedral de San Miguel de Allende</color>.",
        "Para llegar hasta ella tendrás que recorrer cuatro ciudades, juntar fresas y esquivar los peligros del camino.",
        "Tu viaje comienza en <color=" + InterfazUI.Rosa + ">Irapuato, la capital de la fresa</color>."
    };

    private TextMeshProUGUI texto;
    private TextMeshProUGUI aviso;
    private Image estudiante;
    private Sprite[] cuadrosEstudiante;
    private Action alTerminar;
    private bool escribiendo;
    private bool terminado;
    private float inicio;

    public static void Mostrar(Transform padre, Action alTerminar)
    {
        RectTransform raiz = InterfazUI.Estirar(InterfazUI.Crear("Historia", padre));
        Image fondo = InterfazUI.Imagen(raiz, InterfazUI.CargarSprite("UI/menu_fondo"), "Fondo");
        InterfazUI.Estirar(fondo.rectTransform);
        fondo.preserveAspect = false;
        fondo.raycastTarget = true;
        AspectRatioFitter ajuste = fondo.gameObject.AddComponent<AspectRatioFitter>();
        ajuste.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
        ajuste.aspectRatio = 2000f / 858f;
        InterfazUI.Panel(raiz, new Color(0.03f, 0.02f, 0.06f, 0.72f), "Oscurecer").raycastTarget = false;

        PantallaHistoria h = raiz.gameObject.AddComponent<PantallaHistoria>();
        h.alTerminar = alTerminar;

        Image papel = InterfazUI.PapelPicado(raiz, 120f);
        papel.rectTransform.anchorMin = new Vector2(0f, 1f);
        papel.rectTransform.anchorMax = new Vector2(1f, 1f);
        papel.rectTransform.pivot = new Vector2(0.5f, 1f);
        papel.rectTransform.sizeDelta = new Vector2(0f, 120f);
        papel.rectTransform.anchoredPosition = Vector2.zero;

        // Marco con el texto de la historia
        Image placa = InterfazUI.Placa(raiz, "Marco", 1.8f);
        RectTransform marco = InterfazUI.Colocar(placa.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(120, 10), new Vector2(1240, 620));
        RectTransform titulo = InterfazUI.Liston(marco, "LA HISTORIA", 700, 120, 40);
        InterfazUI.Colocar(titulo, new Vector2(0.5f, 1f), new Vector2(0, 55), titulo.sizeDelta);

        h.texto = InterfazUI.Texto(marco, "", 44, InterfazUI.ColorTexto, TextAlignmentOptions.TopLeft, "Texto");
        InterfazUI.Estirar(h.texto.rectTransform);
        h.texto.rectTransform.offsetMin = new Vector2(90, 70);
        h.texto.rectTransform.offsetMax = new Vector2(-90, -100);
        h.texto.lineSpacing = 10f;
        // Se ajusta solo para que la historia siempre quepa dentro del marco
        h.texto.enableAutoSizing = true;
        h.texto.fontSizeMin = 26f;
        h.texto.fontSizeMax = 42f;

        // El Estudiante, listo para el viaje, a la izquierda del marco
        h.cuadrosEstudiante = new[] { InterfazUI.CargarSprite("UI/estudiante_0"), InterfazUI.CargarSprite("UI/estudiante_1") };
        h.estudiante = InterfazUI.Imagen(raiz, h.cuadrosEstudiante[0], "Estudiante");
        InterfazUI.Colocar(h.estudiante.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(-690, -120), new Vector2(260, 400));

        h.aviso = InterfazUI.Texto(raiz, "", 30, InterfazUI.ColorTitulo, TextAlignmentOptions.Center, "Aviso");
        InterfazUI.Colocar(h.aviso.rectTransform, new Vector2(0.5f, 0f), new Vector2(120, 34), new Vector2(1240, 50));
        InterfazUI.Sombra(h.aviso, 0.8f);

        Button omitir = InterfazUI.Boton(raiz, "Omitir introducción", h.Terminar, 500, 100);
        InterfazUI.Colocar((RectTransform)omitir.transform, new Vector2(1f, 0f), new Vector2(-50, 100), new Vector2(500, 100));

        // Si ya la vio, el botón de omitir queda listo para Enter
        if (PlayerPrefs.GetInt(ClaveVista, 0) == 1) InterfazUI.Seleccionar(omitir);

        h.StartCoroutine(h.Escribir());
    }

    void Awake()
    {
        inicio = Time.unscaledTime;
    }

    private IEnumerator Escribir()
    {
        escribiendo = true;
        aviso.text = "Enter / Espacio / A: mostrar todo     ·     Esc / B: omitir";
        texto.text = string.Join("\n\n", Parrafos);
        texto.maxVisibleCharacters = 0;
        texto.ForceMeshUpdate();
        int total = texto.textInfo.characterCount;

        for (float visibles = 0; visibles < total && escribiendo; visibles += LetrasPorSegundo * Time.unscaledDeltaTime)
        {
            texto.maxVisibleCharacters = (int)visibles;
            yield return null;
        }
        texto.maxVisibleCharacters = total;
        escribiendo = false;
        aviso.text = "Enter / Espacio / A: ¡comenzar!";
    }

    void Update()
    {
        // El Estudiante parpadea (dos cuadros de su animación idle)
        int cuadro = Mathf.FloorToInt(Time.unscaledTime * 1.5f) % 2;
        if (cuadrosEstudiante[cuadro] != null) estudiante.sprite = cuadrosEstudiante[cuadro];

        // La misma tecla que abrió la historia no debe saltarla
        if (Time.unscaledTime - inicio < 0.3f) return;

        bool omitir = (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
                      || (Gamepad.current != null && Gamepad.current.buttonEast.wasPressedThisFrame);
        if (omitir)
        {
            Terminar();
            return;
        }

        // Si el botón "Omitir" está seleccionado, Enter lo presiona (lo maneja el EventSystem)
        bool avanzar = (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
                       || (Keyboard.current != null && Keyboard.current.enterKey.wasPressedThisFrame && !OmitirSeleccionado())
                       || (Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame && !OmitirSeleccionado());
        if (!avanzar) return;

        // Primer toque: muestra todo el texto; segundo toque: empieza el juego
        if (escribiendo) escribiendo = false;
        else Terminar();
    }

    private static bool OmitirSeleccionado()
    {
        GameObject sel = UnityEngine.EventSystems.EventSystem.current != null ? UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject : null;
        return sel != null && sel.name.StartsWith("Boton_Omitir");
    }

    private void Terminar()
    {
        if (terminado) return;
        terminado = true;
        PlayerPrefs.SetInt(ClaveVista, 1);
        PlayerPrefs.Save();
        alTerminar?.Invoke();
    }
}
