using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Menú de opciones (desde el menú principal y desde la pausa):
// volumen de música y de efectos, calidad gráfica, pantalla completa, controles y borrar progreso.
// Todo se guarda en PlayerPrefs y se aplica de inmediato
public static class PanelOpciones
{
    public const string ClaveCalidad = "CalidadGrafica";

    private static float ultimoEjemplo;

    private const string TextoControles =
        "<b>Teclado:</b> A / D o las flechas para moverte · Espacio para saltar · Esc para pausar\n" +
        "<b>Control:</b> stick izquierdo para moverte · A para saltar · Start para pausar\n" +
        "Golpea las cajas desde abajo y salta sobre las ratas para aplastarlas.";

    public static RectTransform Abrir(Transform padre, Action alCerrar, bool desdeMenuPrincipal)
    {
        RectTransform caja = InterfazUI.Ventana(padre, "OPCIONES", new Vector2(1300, 1000), InterfazUI.ColorTitulo);

        // Volúmenes
        Etiqueta(caja, "Música", -170);
        Slider musica = InterfazUI.Deslizador(caja, Sonido.VolumenMusica, v => Sonido.VolumenMusica = v);
        InterfazUI.Colocar((RectTransform)musica.transform, new Vector2(0.5f, 1f), new Vector2(200, -180), new Vector2(540, 40));

        Etiqueta(caja, "Efectos", -250);
        Slider efectos = InterfazUI.Deslizador(caja, Sonido.VolumenEfectos, v => Sonido.VolumenEfectos = v);
        InterfazUI.Colocar((RectTransform)efectos.transform, new Vector2(0.5f, 1f), new Vector2(200, -260), new Vector2(540, 40));
        // Mientras se mueve se escucha un ejemplo con el volumen nuevo (sin saturar: uno cada 0.2 s)
        efectos.onValueChanged.AddListener(_ =>
        {
            if (Time.unscaledTime - ultimoEjemplo < 0.2f) return;
            ultimoEjemplo = Time.unscaledTime;
            Sonido.Efecto("fresa", 0.6f);
        });

        // Calidad gráfica: < Media >
        Etiqueta(caja, "Calidad gráfica", -330);
        TextMeshProUGUI calidad = InterfazUI.Texto(caja, NombreCalidad(), 40, InterfazUI.ColorTexto);
        InterfazUI.Colocar(calidad.rectTransform, new Vector2(0.5f, 1f), new Vector2(200, -325), new Vector2(280, 60));
        Button menos = InterfazUI.Boton(caja, "<", () => CambiarCalidad(-1, calidad), 90, 70);
        InterfazUI.Colocar((RectTransform)menos.transform, new Vector2(0.5f, 1f), new Vector2(10, -320), new Vector2(90, 70));
        Button mas = InterfazUI.Boton(caja, ">", () => CambiarCalidad(1, calidad), 90, 70);
        InterfazUI.Colocar((RectTransform)mas.transform, new Vector2(0.5f, 1f), new Vector2(390, -320), new Vector2(90, 70));

        // Pantalla completa
        Button completa = null;
        completa = InterfazUI.Boton(caja, TextoPantalla(), () =>
        {
            Screen.fullScreen = !Screen.fullScreen;
            // El cambio se aplica al siguiente cuadro: se muestra el estado que tendrá
            completa.GetComponentInChildren<TextMeshProUGUI>().text = TextoPantalla(!Screen.fullScreen);
        }, 640, 80);
        InterfazUI.Colocar((RectTransform)completa.transform, new Vector2(0.5f, 1f), new Vector2(0, -420), new Vector2(640, 80));

        // Controles
        TextMeshProUGUI controles = InterfazUI.Texto(caja, TextoControles, 30, InterfazUI.ColorTexto);
        InterfazUI.Colocar(controles.rectTransform, new Vector2(0.5f, 1f), new Vector2(0, -530), new Vector2(1180, 170));

        // Botones inferiores
        RectTransform fila = InterfazUI.Colocar(InterfazUI.Crear("Fila", caja), new Vector2(0.5f, 0f), new Vector2(0, 40), new Vector2(1200, 110));
        HorizontalLayoutGroup h = fila.gameObject.AddComponent<HorizontalLayoutGroup>();
        h.spacing = 40;
        h.childAlignment = TextAnchor.MiddleCenter;
        h.childControlWidth = false;
        h.childControlHeight = false;

        if (desdeMenuPrincipal)
        {
            Button borrar = null;
            bool confirmar = false;
            borrar = InterfazUI.Boton(fila, "Borrar progreso", () =>
            {
                TextMeshProUGUI t = borrar.GetComponentInChildren<TextMeshProUGUI>();
                if (!confirmar)
                {
                    confirmar = true;
                    t.text = "¿Seguro? Otra vez";
                    return;
                }
                Guardado.BorrarTodo();
                t.text = "Progreso borrado";
                borrar.interactable = false;
            });
        }
        Button volver = InterfazUI.Boton(fila, "Volver", () => alCerrar?.Invoke());

        InterfazUI.Seleccionar(musica);
        return caja;
    }

    private static void Etiqueta(RectTransform caja, string texto, float y)
    {
        TextMeshProUGUI t = InterfazUI.Texto(caja, texto, 40, InterfazUI.ColorTexto, TextAlignmentOptions.Right);
        InterfazUI.Colocar(t.rectTransform, new Vector2(0.5f, 1f), new Vector2(-330, y), new Vector2(420, 60));
    }

    private static void CambiarCalidad(int paso, TextMeshProUGUI etiqueta)
    {
        int total = QualitySettings.names.Length;
        int nivel = (QualitySettings.GetQualityLevel() + paso + total) % total;
        QualitySettings.SetQualityLevel(nivel, true);
        PlayerPrefs.SetInt(ClaveCalidad, nivel);
        PlayerPrefs.Save();
        etiqueta.text = NombreCalidad();
    }

    private static string NombreCalidad()
    {
        string[] nombres = QualitySettings.names;
        int nivel = QualitySettings.GetQualityLevel();
        return nivel >= 0 && nivel < nombres.Length ? nombres[nivel] : "-";
    }

    private static string TextoPantalla() => TextoPantalla(Screen.fullScreen);
    private static string TextoPantalla(bool completa) => "Pantalla completa: " + (completa ? "Sí" : "No");
}
