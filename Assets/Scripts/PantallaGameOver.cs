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
        RectTransform caja = InterfazUI.Ventana(canvas.transform, "GAME OVER", new Vector2(1000, 760), new Color(1f, 0.3f, 0.3f));

        string resumen = "Te quedaste sin vidas.\n\n" +
                         $"Puntos: <color=#FFC832>{puntos}</color>\n" +
                         $"Tiempo: {Partida.FormatoTiempo(tiempo)}\n" +
                         (nuevoRecord ? "<color=#FFC832>¡Nuevo récord!</color>" : $"Récord: {Guardado.PuntajeMaximo}");
        TextMeshProUGUI texto = InterfazUI.Texto(caja, resumen, 44, InterfazUI.ColorTexto);
        InterfazUI.Colocar(texto.rectTransform, new Vector2(0.5f, 1f), new Vector2(0, -160), new Vector2(900, 300));

        RectTransform botones = InterfazUI.Columna(caja, new Vector2(0.5f, 0f), new Vector2(0, 40));
        botones.sizeDelta = new Vector2(600, 250);
        Button reintentar = InterfazUI.Boton(botones, "Reintentar nivel", Partida.Reintentar);
        InterfazUI.Boton(botones, "Menú principal", Partida.IrAlMenu);
        InterfazUI.Seleccionar(reintentar);
    }
}
