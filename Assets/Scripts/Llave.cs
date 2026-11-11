using System;
using UnityEngine;

// Llave del nivel (HU15). Es estática para que la meta (LevelGoal) y el HUD la consulten sin referencias
public static class Llave
{
    public static bool Tiene { get; private set; }

    // El HUD se suscribe aquí para mostrar u ocultar el ícono de la llave
    public static event Action<bool> LlaveCambiada;

    public static void Obtener()
    {
        Tiene = true;
        LlaveCambiada?.Invoke(true);
    }

    // Partida nueva o reintentar después de Game Over
    public static void Reiniciar()
    {
        Tiene = false;
        LlaveCambiada?.Invoke(false);
    }

    // Se ejecuta al entrar a Play, aunque esté desactivado "Reload Domain", para que cada partida empiece sin llave
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ReiniciarAlIniciar()
    {
        Tiene = false;
        LlaveCambiada = null;
    }
}
