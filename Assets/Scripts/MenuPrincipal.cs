using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Menú principal: Jugar, Continuar, Opciones, Créditos y Salir.
// "Continuar" solo se activa si hay una partida guardada. "Jugar" muestra la introducción de la historia.
// Va en la escena MenuPrincipal; construye su interfaz al iniciar
public class MenuPrincipal : MonoBehaviour
{
    private Canvas canvas;
    private RectTransform botones;
    private RectTransform ventana;

    void Start()
    {
        Time.timeScale = 1f;
        Sonido.Musica("menu");
        canvas = InterfazUI.CrearCanvas("Menu", 0);

        // Fondo: el Centro Histórico de Irapuato, oscurecido para que se lea el menú
        Image fondo = InterfazUI.Imagen(canvas.transform, InterfazUI.CargarSprite("UI/menu_fondo"), "Fondo");
        InterfazUI.Estirar(fondo.rectTransform);
        fondo.preserveAspect = false;
        AspectRatioFitter ajuste = fondo.gameObject.AddComponent<AspectRatioFitter>();
        ajuste.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
        ajuste.aspectRatio = 2000f / 858f;
        InterfazUI.Panel(canvas.transform, new Color(0.05f, 0.03f, 0.1f, 0.45f), "Oscurecer").raycastTarget = false;

        Image logo = InterfazUI.Imagen(canvas.transform, InterfazUI.CargarSprite("UI/logo_titulo"), "Logo");
        InterfazUI.Colocar(logo.rectTransform, new Vector2(0.5f, 1f), new Vector2(0, -20), new Vector2(740, 425));

        botones = InterfazUI.Columna(canvas.transform, new Vector2(0.5f, 0f), new Vector2(0, 90), 12f);
        botones.sizeDelta = new Vector2(600, 530);   // 5 botones de 96 + espacios: no toca el logotipo
        Button jugar = InterfazUI.Boton(botones, "Jugar", Jugar, 480, 96);
        Button continuar = InterfazUI.Boton(botones, "Continuar", Partida.Continuar, 480, 96);
        continuar.interactable = Guardado.HayPartida;
        InterfazUI.Boton(botones, "Opciones", AbrirOpciones, 480, 96);
        InterfazUI.Boton(botones, "Créditos", AbrirCreditos, 480, 96);
        InterfazUI.Boton(botones, "Salir", Salir, 480, 96);

        string pie = $"Récord: {Guardado.PuntajeMaximo}   ·   Nivel desbloqueado: {Guardado.NivelDesbloqueado}";
        TextMeshProUGUI textoPie = InterfazUI.Texto(canvas.transform, pie, 30, InterfazUI.ColorTexto);
        InterfazUI.Colocar(textoPie.rectTransform, new Vector2(0.5f, 0f), new Vector2(0, 24), new Vector2(1200, 50));

        InterfazUI.Seleccionar(continuar.interactable ? continuar : jugar);
    }

    private void Jugar()
    {
        botones.gameObject.SetActive(false);
        PantallaHistoria.Mostrar(canvas.transform, Partida.NuevaPartida);
    }

    private void AbrirOpciones()
    {
        botones.gameObject.SetActive(false);
        ventana = PanelOpciones.Abrir(canvas.transform, CerrarVentana, true);
    }

    private void AbrirCreditos()
    {
        botones.gameObject.SetActive(false);
        ventana = PanelCreditos.Abrir(canvas.transform, CerrarVentana);
    }

    private void CerrarVentana()
    {
        InterfazUI.CerrarVentana(ventana);
        ventana = null;
        botones.gameObject.SetActive(true);

        // Si se borró el progreso, "Continuar" ya no aplica
        Button[] lista = botones.GetComponentsInChildren<Button>();
        lista[1].interactable = Guardado.HayPartida;
        InterfazUI.Seleccionar(lista[0]);
    }

    private void Salir()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
