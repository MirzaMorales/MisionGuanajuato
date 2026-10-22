using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Menú principal: Jugar, Continuar, Opciones, Créditos y Salir.
// "Continuar" solo se activa si hay una partida guardada. "Jugar" muestra la introducción de la historia.
// Ambiente: el Centro Histórico de fondo desplazándose despacio, papel picado, el logotipo meciéndose
// y el Estudiante esperando a la izquierda. Va en la escena MenuPrincipal; construye su interfaz al iniciar
public class MenuPrincipal : MonoBehaviour
{
    private Canvas canvas;
    private RectTransform botones;
    private RectTransform ventana;
    private RectTransform fondo;
    private RectTransform logo;
    private Image estudiante;
    private Sprite[] cuadrosEstudiante;

    void Start()
    {
        Time.timeScale = 1f;
        Sonido.Musica("menu");
        canvas = InterfazUI.CrearCanvas("Menu", 0);

        // Fondo un poco más grande que la pantalla para poder desplazarlo
        Image imgFondo = InterfazUI.Imagen(canvas.transform, InterfazUI.CargarSprite("UI/menu_fondo"), "Fondo");
        fondo = InterfazUI.Estirar(imgFondo.rectTransform);
        imgFondo.preserveAspect = false;
        AspectRatioFitter ajuste = imgFondo.gameObject.AddComponent<AspectRatioFitter>();
        ajuste.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
        ajuste.aspectRatio = 2000f / 858f;
        fondo.localScale = new Vector3(1.12f, 1.12f, 1f);
        InterfazUI.Panel(canvas.transform, new Color(0.05f, 0.03f, 0.1f, 0.35f), "Oscurecer").raycastTarget = false;

        Image papel = InterfazUI.PapelPicado(canvas.transform, 120f);
        papel.rectTransform.anchorMin = new Vector2(0f, 1f);
        papel.rectTransform.anchorMax = new Vector2(1f, 1f);
        papel.rectTransform.pivot = new Vector2(0.5f, 1f);
        papel.rectTransform.sizeDelta = new Vector2(0f, 120f);
        papel.rectTransform.anchoredPosition = Vector2.zero;

        logo = InterfazUI.Imagen(canvas.transform, InterfazUI.CargarSprite("UI/logo_titulo"), "Logo").rectTransform;
        InterfazUI.Colocar(logo, new Vector2(0.5f, 1f), new Vector2(0, -95), new Vector2(700, 402));

        // El Estudiante, abajo a la izquierda, parado en la banqueta del fondo
        cuadrosEstudiante = new[] { InterfazUI.CargarSprite("UI/estudiante_0"), InterfazUI.CargarSprite("UI/estudiante_1") };
        estudiante = InterfazUI.Imagen(canvas.transform, cuadrosEstudiante[0], "Estudiante");
        InterfazUI.Colocar(estudiante.rectTransform, new Vector2(0f, 0f), new Vector2(170, 190), new Vector2(240, 368));

        botones = InterfazUI.Columna(canvas.transform, new Vector2(0.5f, 0f), new Vector2(0, 40), 10f);
        botones.sizeDelta = new Vector2(600, 500);   // 5 botones de 92 + espacios: no toca el logotipo
        Button jugar = InterfazUI.Boton(botones, "Jugar", Jugar, 470, 92);
        Button continuar = InterfazUI.Boton(botones, "Continuar", Partida.Continuar, 470, 92);
        continuar.interactable = Guardado.HayPartida;
        InterfazUI.Boton(botones, "Opciones", AbrirOpciones, 470, 92);
        InterfazUI.Boton(botones, "Créditos", AbrirCreditos, 470, 92);
        InterfazUI.Boton(botones, "Salir", Salir, 470, 92);

        // Récord y nivel desbloqueado en una placa pequeña
        Image placa = InterfazUI.Placa(canvas.transform, "Record", 2.6f);
        InterfazUI.Colocar(placa.rectTransform, new Vector2(1f, 0f), new Vector2(-30, 30), new Vector2(430, 120));
        string pie = $"<color={InterfazUI.Amarillo}>Récord:</color> {Guardado.PuntajeMaximo}\n<color={InterfazUI.Amarillo}>Nivel desbloqueado:</color> {Guardado.NivelDesbloqueado}";
        TextMeshProUGUI textoPie = InterfazUI.Texto(placa.transform, pie, 28, InterfazUI.ColorTexto);
        InterfazUI.Estirar(textoPie.rectTransform);
        textoPie.rectTransform.offsetMin = new Vector2(24, 14);
        textoPie.rectTransform.offsetMax = new Vector2(-24, -14);

        InterfazUI.Seleccionar(continuar.interactable ? continuar : jugar);
    }

    void Update()
    {
        float t = Time.unscaledTime;
        // Fondo que se desplaza muy despacio de un lado a otro
        if (fondo != null) fondo.anchoredPosition = new Vector2(Mathf.Sin(t * 0.08f) * 90f, 0f);
        // Logotipo meciéndose
        if (logo != null)
        {
            logo.anchoredPosition = new Vector2(0, -95 + Mathf.Sin(t * 1.6f) * 8f);
            logo.localRotation = Quaternion.Euler(0, 0, Mathf.Sin(t * 0.9f) * 1.5f);
        }
        // El Estudiante parpadea
        if (estudiante != null && cuadrosEstudiante[0] != null)
        {
            estudiante.sprite = cuadrosEstudiante[Mathf.FloorToInt(t * 1.5f) % 2];
        }
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
