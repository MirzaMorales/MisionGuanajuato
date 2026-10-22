using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// Pantalla de carga al abrir el juego: primero el logotipo del equipo con sus autores,
// después el logotipo del juego, y pasa sola al menú principal. Cualquier tecla la omite.
// Va en la escena Splash (la primera de Build Settings)
public class PantallaSplash : MonoBehaviour
{
    private bool saliendo;

    void Start()
    {
        Canvas canvas = InterfazUI.CrearCanvas("Splash", 0);
        InterfazUI.Panel(canvas.transform, new Color(0.05f, 0.03f, 0.08f), "Fondo");

        // Equipo
        RectTransform equipo = InterfazUI.Estirar(InterfazUI.Crear("Equipo", canvas.transform));
        CanvasGroup grupoEquipo = equipo.gameObject.AddComponent<CanvasGroup>();
        grupoEquipo.alpha = 0f;
        Image emblema = InterfazUI.Imagen(equipo, InterfazUI.CargarSprite("UI/emblema"), "Emblema");
        InterfazUI.Colocar(emblema.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0, 130), new Vector2(420, 428));
        TextMeshProUGUI autores = InterfazUI.Texto(equipo,
            "Un juego de\n<color=#FFC832>Mirza Natzielly Morales Lezama</color>\ny <color=#FFC832>Juan Gustavo Ángel Cruz Méndez</color>",
            42, InterfazUI.ColorTexto);
        InterfazUI.Colocar(autores.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0, -230), new Vector2(1400, 200));
        TextMeshProUGUI escuela = InterfazUI.Texto(equipo, "Universidad Tecnológica del Norte de Guanajuato · 2026", 28, InterfazUI.ColorTexto);
        InterfazUI.Colocar(escuela.rectTransform, new Vector2(0.5f, 0f), new Vector2(0, 50), new Vector2(1400, 50));

        // Juego
        RectTransform juego = InterfazUI.Estirar(InterfazUI.Crear("Juego", canvas.transform));
        CanvasGroup grupoJuego = juego.gameObject.AddComponent<CanvasGroup>();
        grupoJuego.alpha = 0f;
        Image logo = InterfazUI.Imagen(juego, InterfazUI.CargarSprite("UI/logo_titulo"), "Logo");
        InterfazUI.Colocar(logo.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0, 40), new Vector2(1200, 690));

        StartCoroutine(Secuencia(grupoEquipo, grupoJuego));
    }

    private IEnumerator Secuencia(CanvasGroup equipo, CanvasGroup juego)
    {
        yield return Fundir(equipo, 0f, 1f, 0.6f);
        yield return Esperar(1.8f);
        yield return Fundir(equipo, 1f, 0f, 0.5f);
        yield return Fundir(juego, 0f, 1f, 0.6f);
        yield return Esperar(1.8f);
        IrAlMenu();
    }

    void Update()
    {
        bool tecla = (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame)
                     || (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
                     || (Gamepad.current != null && (Gamepad.current.buttonSouth.wasPressedThisFrame || Gamepad.current.startButton.wasPressedThisFrame));
        if (tecla) IrAlMenu();
    }

    private void IrAlMenu()
    {
        if (saliendo) return;
        saliendo = true;
        TransicionEscena.Cargar(Partida.EscenaMenu);
    }

    private IEnumerator Esperar(float segundos)
    {
        for (float t = 0f; t < segundos && !saliendo; t += Time.unscaledDeltaTime) yield return null;
    }

    private IEnumerator Fundir(CanvasGroup grupo, float desde, float hasta, float duracion)
    {
        for (float t = 0f; t < duracion && !saliendo; t += Time.unscaledDeltaTime)
        {
            grupo.alpha = Mathf.Lerp(desde, hasta, t / duracion);
            yield return null;
        }
        grupo.alpha = hasta;
    }
}
