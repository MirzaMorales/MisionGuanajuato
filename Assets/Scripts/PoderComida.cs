using UnityEngine;

// Estado del poder de lanzar comida (HU29): la mochila lo activa y cualquier golpe lo desactiva sin quitar vida.
// Se conserva entre escenas, igual que el tamaño grande. En la escena del jefe siempre se puede lanzar
public static class PoderComida
{
    // Lo tiene el Estudiante porque recogió una mochila
    public static bool Activo { get; private set; }

    // Lo pone Partida al cargar cada escena: con el jefe en la escena el poder no se pierde
    public static bool JefeEnEscena { get; set; }

    // ¿Puede lanzar comida ahora mismo?
    public static bool Disponible => Activo || JefeEnEscena;

    public static void Activar()
    {
        if (Activo) return;
        Activo = true;
        Sonido.Efecto("crecer");
    }

    public static void Perder()
    {
        Activo = false;
        Sonido.Efecto("encoger");
    }

    public static void Reiniciar()
    {
        Activo = false;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ReiniciarAlIniciar()
    {
        Activo = false;
        JefeEnEscena = false;
    }
}
