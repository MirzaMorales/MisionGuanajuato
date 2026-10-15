using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// Pausa durante el juego: Esc en el teclado o Start en el control.
// Permite reanudar, abrir las opciones o volver al menú principal
public class MenuPausa : MonoBehaviour
{
    private Canvas canvas;
    private RectTransform ventana;
    private RectTransform ventanaOpciones;

    public static bool EnPausa { get; private set; }

    public static void Crear()
    {
        EnPausa = false;
        new GameObject("MenuPausa").AddComponent<MenuPausa>();
    }

    void Update()
    {
        bool tecla = (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
                     || (Gamepad.current != null && Gamepad.current.startButton.wasPressedThisFrame);
        if (!tecla) return;

        // Con una ventana de opciones abierta, Esc solo la cierra
        if (ventanaOpciones != null)
        {
            CerrarOpciones();
            return;
        }

        if (EnPausa) Reanudar();
        else if (!Partida.Terminada) Pausar();
    }

    private void Pausar()
    {
        EnPausa = true;
        Time.timeScale = 0f;

        if (canvas == null) canvas = InterfazUI.CrearCanvas("Pausa", 40);
        ventana = InterfazUI.Ventana(canvas.transform, "PAUSA", new Vector2(800, 700), InterfazUI.ColorTitulo);
        RectTransform botones = InterfazUI.Columna(ventana, new Vector2(0.5f, 1f), new Vector2(0, -170));
        Button reanudar = InterfazUI.Boton(botones, "Reanudar", Reanudar);
        InterfazUI.Boton(botones, "Opciones", AbrirOpciones);
        InterfazUI.Boton(botones, "Menú principal", () => { EnPausa = false; Partida.IrAlMenu(); });
        InterfazUI.Seleccionar(reanudar);
    }

    private void Reanudar()
    {
        EnPausa = false;
        Time.timeScale = 1f;
        InterfazUI.CerrarVentana(ventana);
        ventana = null;
    }

    private void AbrirOpciones()
    {
        ventanaOpciones = PanelOpciones.Abrir(canvas.transform, CerrarOpciones, false);
    }

    private void CerrarOpciones()
    {
        InterfazUI.CerrarVentana(ventanaOpciones);
        ventanaOpciones = null;
        if (ventana != null) InterfazUI.Seleccionar(ventana.GetComponentInChildren<Button>());
    }

    void OnDestroy()
    {
        EnPausa = false;
    }
}
