using UnityEngine;

// Guardado del progreso en PlayerPrefs: la partida en curso (para "Continuar"),
// el puntaje máximo y el nivel más avanzado que se ha desbloqueado
public static class Guardado
{
    private const string Escena = "Partida_Escena";
    private const string Vidas = "Partida_Vidas";
    private const string Puntos = "Partida_Puntos";
    private const string Grande = "Partida_Grande";
    private const string Tiempo = "Partida_Tiempo";
    private const string Record = "PuntajeMaximo";
    private const string Desbloqueado = "NivelDesbloqueado";

    // Hay partida para continuar si se guardó una escena que existe en Build Settings
    public static bool HayPartida
    {
        get
        {
            string escena = PlayerPrefs.GetString(Escena, "");
            return escena != "" && Application.CanStreamedLevelBeLoaded(escena);
        }
    }

    public static string EscenaGuardada => PlayerPrefs.GetString(Escena, "");
    public static int VidasGuardadas => PlayerPrefs.GetInt(Vidas, 3);
    public static int PuntosGuardados => PlayerPrefs.GetInt(Puntos, 0);
    public static bool GrandeGuardado => PlayerPrefs.GetInt(Grande, 0) == 1;
    public static float TiempoGuardado => PlayerPrefs.GetFloat(Tiempo, 0f);

    public static int PuntajeMaximo => PlayerPrefs.GetInt(Record, 0);
    public static int NivelDesbloqueado => PlayerPrefs.GetInt(Desbloqueado, 1);

    // Se llama al entrar a cada escena de un nivel
    public static void GuardarPartida(string escena, int vidas, int puntos, bool grande, float tiempo)
    {
        PlayerPrefs.SetString(Escena, escena);
        PlayerPrefs.SetInt(Vidas, vidas);
        PlayerPrefs.SetInt(Puntos, puntos);
        PlayerPrefs.SetInt(Grande, grande ? 1 : 0);
        PlayerPrefs.SetFloat(Tiempo, tiempo);
        PlayerPrefs.Save();
    }

    public static void BorrarPartida()
    {
        PlayerPrefs.DeleteKey(Escena);
        PlayerPrefs.DeleteKey(Vidas);
        PlayerPrefs.DeleteKey(Puntos);
        PlayerPrefs.DeleteKey(Grande);
        PlayerPrefs.DeleteKey(Tiempo);
        PlayerPrefs.Save();
    }

    // Devuelve true si el puntaje es un nuevo récord
    public static bool RegistrarPuntaje(int puntos)
    {
        if (puntos <= PuntajeMaximo) return false;
        PlayerPrefs.SetInt(Record, puntos);
        PlayerPrefs.Save();
        return true;
    }

    public static void DesbloquearNivel(int nivel)
    {
        if (nivel <= NivelDesbloqueado) return;
        PlayerPrefs.SetInt(Desbloqueado, nivel);
        PlayerPrefs.Save();
    }

    // Borra todo el progreso (opción "Borrar progreso" del menú de opciones)
    public static void BorrarTodo()
    {
        BorrarPartida();
        PlayerPrefs.DeleteKey(Record);
        PlayerPrefs.DeleteKey(Desbloqueado);
        PlayerPrefs.Save();
    }
}
