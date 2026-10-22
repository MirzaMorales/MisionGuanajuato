using System;
using UnityEngine;

// Puntuación total del jugador. Es estática para que cualquier script (coleccionables, HUD) la use sin referencias
public static class Puntuacion
{
    public static int Total { get; private set; }

    // El HUD (HU05) se suscribe aquí para actualizarse en tiempo real
    public static event Action<int> PuntosCambiados;

    public static void Sumar(int puntos)
    {
        Total += puntos;
        PuntosCambiados?.Invoke(Total);
    }

    // Partida nueva desde el menú o al reintentar después de Game Over
    public static void ReiniciarPuntos()
    {
        Total = 0;
        PuntosCambiados?.Invoke(Total);
    }

    // "Continuar": recupera los puntos de la partida guardada
    public static void Establecer(int puntos)
    {
        Total = Mathf.Max(0, puntos);
        PuntosCambiados?.Invoke(Total);
    }

    // Se ejecuta al entrar a Play, aunque esté desactivado "Reload Domain", para que cada partida empiece en 0
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void Reiniciar()
    {
        Total = 0;
        PuntosCambiados = null;
    }
}
