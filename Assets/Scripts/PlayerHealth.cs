using System;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private int vidasMaximas = 3;

    private int vidasActuales;

    // Otros sistemas (HUD, checkpoint, pantalla de Game Over) se suscriben a estos eventos
    public event Action<int> VidasCambiadas;
    public event Action GameOver;

    public int VidasActuales => vidasActuales;
    public bool EstaMuerto => vidasActuales <= 0;

    void Awake()
    {
        vidasActuales = vidasMaximas;
    }

    void Start()
    {
        // Start y no Awake: así los demás scripts ya alcanzaron a suscribirse
        VidasCambiadas?.Invoke(vidasActuales);
    }

    // Lo llamará la detección de daño (HU07) al tocar un enemigo u obstáculo
    [ContextMenu("Probar: recibir daño")]
    public void RecibirDano()
    {
        if (EstaMuerto) return;

        vidasActuales--;
        VidasCambiadas?.Invoke(vidasActuales);

        if (EstaMuerto)
        {
            Debug.Log("Game Over");
            GameOver?.Invoke();
        }
    }
}
