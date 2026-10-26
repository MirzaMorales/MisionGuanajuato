using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Victoria del nivel: "pantalla breve que indique la ciudad superada" (GDD).
// Muestra la ciudad, una línea de la historia, los puntos, el tiempo y el récord.
// "Siguiente nivel" solo se activa cuando el siguiente nivel ya existe en Build Settings
public static class PantallaFinNivel
{
    public static void Mostrar(Partida.InfoEscena info, int puntos, float tiempo, bool nuevoRecord)
    {
        Canvas canvas = InterfazUI.CrearCanvas("FinNivel", 50);
        // En la última ciudad ya no hay a dónde seguir: se celebra el final del juego
        bool esElFinal = string.IsNullOrEmpty(info.siguienteNivel);
        string titulo = esElFinal ? "¡MISIÓN CUMPLIDA!" : $"¡{info.ciudad.ToUpper()} SUPERADO!";
        RectTransform caja = InterfazUI.Ventana(canvas.transform, titulo, new Vector2(1200, 860), InterfazUI.ColorTitulo);

        TextMeshProUGUI historia = InterfazUI.Texto(caja, info.mensajeVictoria ?? "", 38, InterfazUI.ColorTexto);
        historia.fontStyle = FontStyles.Italic;
        InterfazUI.Colocar(historia.rectTransform, new Vector2(0.5f, 1f), new Vector2(0, -150), new Vector2(1100, 120));

        string resumen = $"Nivel {info.numeroNivel} completado\n\n" +
                         $"Puntos: <color=#FFC832>{puntos}</color>\n" +
                         $"Tiempo: {Partida.FormatoTiempo(tiempo)}\n" +
                         (nuevoRecord ? "<color=#FFC832>¡Nuevo récord!</color>" : $"Récord: {Guardado.PuntajeMaximo}");
        TextMeshProUGUI texto = InterfazUI.Texto(caja, resumen, 42, InterfazUI.ColorTexto);
        InterfazUI.Colocar(texto.rectTransform, new Vector2(0.5f, 1f), new Vector2(0, -290), new Vector2(1100, 290));

        RectTransform botones = InterfazUI.Columna(caja, new Vector2(0.5f, 0f), new Vector2(0, 40));
        botones.sizeDelta = new Vector2(700, 250);
        bool hay = Partida.HaySiguienteNivel;
        Button siguiente = null;
        if (!esElFinal)
        {
            siguiente = InterfazUI.Boton(botones, hay ? "Siguiente nivel" : "Próximamente: Guanajuato", Partida.SiguienteNivel, 640);
            siguiente.interactable = hay;
        }
        Button menu = InterfazUI.Boton(botones, "Menú principal", Partida.IrAlMenu, 640);
        InterfazUI.Seleccionar(hay && siguiente != null ? siguiente : menu);
    }
}
