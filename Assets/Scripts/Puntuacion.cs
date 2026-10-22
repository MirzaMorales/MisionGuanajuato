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

    // Se ejecuta al entrar a Play, aunque esté desactivado "Reload Domain", para que cada partida empiece en 0
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void Reiniciar()
    {
        Total = 0;
        PuntosCambiados = null;
    }
}
