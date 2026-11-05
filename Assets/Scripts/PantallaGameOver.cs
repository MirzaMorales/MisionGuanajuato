using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Derrota: aparece al quedarse sin vidas. Muestra el puntaje final, el tiempo y el récord,
// y permite reintentar el nivel (con las vidas completas) o volver al menú principal
public static class PantallaGameOver
{
    public static void Mostrar(int puntos, float tiempo, bool nuevoRecord)
    {
        Canvas canvas = InterfazUI.CrearCanvas("GameOver", 50);
        // Con la ilustración la ventana es más ancha: el dibujo a la izquierda y el texto a la derecha
        Sprite arte = InterfazUI.CargarSprite("UI/arte_gameover");
        bool conArte = arte != null;
        RectTransform caja = InterfazUI.Ventana(canvas.transform, "GAME OVER", new Vector2(conArte ? 1400 : 1000, 760), new Color(1f, 0.3f, 0.3f));

        string resumen = "Te quedaste sin vidas.\n\n" +
                         $"Puntos: <color=#FFC832>{puntos}</color>\n" +
                         $"Tiempo: {Partida.FormatoTiempo(tiempo)}\n" +
                         (nuevoRecord ? "<color=#FFC832>¡Nuevo récord!</color>" : $"Récord: {Guardado.PuntajeMaximo}");
        TextMeshProUGUI texto = InterfazUI.Texto(caja, resumen, 44, InterfazUI.ColorTexto);
        InterfazUI.Colocar(texto.rectTransform, new Vector2(0.5f, 1f), new Vector2(conArte ? 270 : 0, -160), new Vector2(conArte ? 700 : 900, 300));

        if (conArte)
        {
            Image dibujo = InterfazUI.Imagen(caja, arte, "Arte");
            dibujo.preserveAspect = true;
            InterfazUI.Colocar(dibujo.rectTransform, new Vector2(0.5f, 1f), new Vector2(-420, -300), new Vector2(420, 340));
        }

        RectTransform botones = InterfazUI.Columna(caja, new Vector2(0.5f, 0f), new Vector2(0, 40));
        botones.sizeDelta = new Vector2(600, 250);
        Button reintentar = InterfazUI.Boton(botones, "Reintentar nivel", Partida.Reintentar);
        InterfazUI.Boton(botones, "Menú principal", Partida.IrAlMenu);
        InterfazUI.Seleccionar(reintentar);
    }
}
