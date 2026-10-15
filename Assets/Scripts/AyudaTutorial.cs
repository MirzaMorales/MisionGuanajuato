using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// Recuadro de ayudas del tutorial: abajo al centro de la pantalla (sobre el asfalto, donde no hay
// nada importante). Muestra solo la ayuda (CartelTutorial) más cercana al jugador.
// - Tecla H o botón Select del control: oculta o muestra las ayudas al momento.
// - También se puede cambiar en Opciones. La preferencia se guarda.
// Lo crea el HUD en cada escena de nivel
public class AyudaTutorial : MonoBehaviour
{
    private const string Clave = "MostrarAyudas";

    private CanvasGroup grupo;
    private RectTransform caja;
    private TextMeshProUGUI texto;
    private Transform jugador;
    private CartelTutorial actual;
    private float alfa;
    private float pulso;

    public static bool Activa
    {
        get => PlayerPrefs.GetInt(Clave, 1) == 1;
        set
        {
            PlayerPrefs.SetInt(Clave, value ? 1 : 0);
            PlayerPrefs.Save();
        }
    }

    public static void Crear(Transform canvas)
    {
        Image placa = InterfazUI.Placa(canvas, "Ayudas", 3.2f);
        AyudaTutorial a = placa.gameObject.AddComponent<AyudaTutorial>();
        a.Construir(placa);
    }

    private void Construir(Image placa)
    {
        caja = InterfazUI.Colocar(placa.rectTransform, new Vector2(0.5f, 0f), new Vector2(0, 36), new Vector2(1060, 100));
        grupo = gameObject.AddComponent<CanvasGroup>();
        grupo.alpha = 0f;
        grupo.blocksRaycasts = false;

        // El alto se ajusta al texto; el ancho es fijo
        VerticalLayoutGroup v = gameObject.AddComponent<VerticalLayoutGroup>();
        v.padding = new RectOffset(48, 48, 26, 20);
        v.spacing = 4;
        v.childAlignment = TextAnchor.MiddleCenter;
        v.childControlWidth = true;
        v.childControlHeight = true;
        v.childForceExpandWidth = true;
        v.childForceExpandHeight = false;
        ContentSizeFitter ajuste = gameObject.AddComponent<ContentSizeFitter>();
        ajuste.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        texto = InterfazUI.Texto(transform, "", 32, InterfazUI.ColorTexto, TextAlignmentOptions.Center, "Mensaje");

        TextMeshProUGUI pista = InterfazUI.Texto(transform, $"<color={InterfazUI.Amarillo}>H</color> / Select: ocultar ayudas", 20,
                                                 new Color(1f, 0.95f, 0.84f, 0.7f), TextAlignmentOptions.Right, "Pista");
        pista.outlineWidth = 0.1f;
    }

    void Update()
    {
        bool alternar = (Keyboard.current != null && Keyboard.current.hKey.wasPressedThisFrame)
                        || (Gamepad.current != null && Gamepad.current.selectButton.wasPressedThisFrame);
        if (alternar && Time.timeScale > 0f) Activa = !Activa;

        if (jugador == null)
        {
            PlayerController pc = FindFirstObjectByType<PlayerController>();
            if (pc != null) jugador = pc.transform;
        }

        CartelTutorial cercano = Activa && Time.timeScale > 0f ? BuscarCercano() : null;
        if (cercano != null && cercano != actual)
        {
            actual = cercano;
            texto.text = cercano.Mensaje;
            pulso = 1f;
        }

        alfa = Mathf.MoveTowards(alfa, cercano != null ? 1f : 0f, Time.unscaledDeltaTime * 4f);
        grupo.alpha = alfa;
        // Sube un poco al aparecer y da un pequeño pulso al cambiar de mensaje
        pulso = Mathf.MoveTowards(pulso, 0f, Time.unscaledDeltaTime * 3f);
        caja.anchoredPosition = new Vector2(0, 36 - (1f - alfa) * 30f);
        float e = 1f + 0.04f * pulso;
        caja.localScale = new Vector3(e, e, 1f);
        if (alfa <= 0f) actual = null;
    }

    private CartelTutorial BuscarCercano()
    {
        if (jugador == null) return null;
        CartelTutorial mejor = null;
        float menor = float.MaxValue;
        foreach (CartelTutorial c in CartelTutorial.Todos)
        {
            float d = Mathf.Abs(jugador.position.x - c.transform.position.x);
            if (d < c.DistanciaVisible && d < menor)
            {
                menor = d;
                mejor = c;
            }
        }
        return mejor;
    }
}
