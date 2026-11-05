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
        // Con la ilustración la ventana es más ancha: el dibujo a la izquierda y los textos a la derecha
        Sprite arte = InterfazUI.CargarSprite("UI/arte_nivel");
        bool conArte = arte != null;
        float desplazamiento = conArte ? 250f : 0f;
        RectTransform caja = InterfazUI.Ventana(canvas.transform, titulo, new Vector2(conArte ? 1500 : 1200, 860), InterfazUI.ColorTitulo);

        if (conArte)
        {
            Image dibujo = InterfazUI.Imagen(caja, arte, "Arte");
            dibujo.preserveAspect = true;
            InterfazUI.Colocar(dibujo.rectTransform, new Vector2(0.5f, 1f), new Vector2(-470, -340), new Vector2(520, 400));
        }

        TextMeshProUGUI historia = InterfazUI.Texto(caja, info.mensajeVictoria ?? "", 38, InterfazUI.ColorTexto);
        historia.fontStyle = FontStyles.Italic;
        InterfazUI.Colocar(historia.rectTransform, new Vector2(0.5f, 1f), new Vector2(desplazamiento, -150), new Vector2(conArte ? 820 : 1100, conArte ? 130 : 120));

        string resumen = $"Nivel {info.numeroNivel} completado\n\n" +
                         $"Puntos: <color=#FFC832>{puntos}</color>\n" +
                         $"Tiempo: {Partida.FormatoTiempo(tiempo)}\n" +
                         (nuevoRecord ? "<color=#FFC832>¡Nuevo récord!</color>" : $"Récord: {Guardado.PuntajeMaximo}");
        TextMeshProUGUI texto = InterfazUI.Texto(caja, resumen, 42, InterfazUI.ColorTexto);
        InterfazUI.Colocar(texto.rectTransform, new Vector2(0.5f, 1f), new Vector2(desplazamiento, -290), new Vector2(conArte ? 820 : 1100, 290));

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
