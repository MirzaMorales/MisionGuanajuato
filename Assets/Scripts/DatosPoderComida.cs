using UnityEngine;

// Imágenes del poder de lanzar comida (fresa, charamusca y nieve) y de la mochila que lo da.
// Vive en Resources para que el juego pueda dárselo al Estudiante en cualquier nivel sin tocar las escenas
public class DatosPoderComida : ScriptableObject
{
    public Sprite[] comidas;
    public Sprite[] mochila;
    public Sprite cajaPoder;
    public Sprite corazon;

    private static DatosPoderComida instancia;

    public static DatosPoderComida Actual
    {
        get
        {
            if (instancia == null) instancia = Resources.Load<DatosPoderComida>("DatosPoderComida");
            return instancia;
        }
    }
}
