using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField] private TMP_Text textoPuntuacion;
    [SerializeField] private string prefijo = "Puntos: ";

    void OnEnable()
    {
        Puntuacion.PuntosCambiados += ActualizarPuntuacion;
        // Muestra el valor actual desde el primer frame (al iniciar el nivel es 0)
        ActualizarPuntuacion(Puntuacion.Total);
    }

    void OnDisable()
    {
        Puntuacion.PuntosCambiados -= ActualizarPuntuacion;
    }

    private void ActualizarPuntuacion(int total)
    {
        if (textoPuntuacion != null) textoPuntuacion.text = prefijo + total;
    }
}
