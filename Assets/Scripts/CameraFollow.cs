using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform objetivo;
    [SerializeField] private float tiempoSuavizado = 0.15f;

    private Vector3 velocidadActual;

    void LateUpdate()
    {
        if (objetivo == null) return;

        // Solo seguimos en X e Y; la Z de la cámara se queda como está
        Vector3 destino = new Vector3(
            objetivo.position.x,
            objetivo.position.y,
            transform.position.z
        );

        transform.position = Vector3.SmoothDamp(
            transform.position, destino, ref velocidadActual, tiempoSuavizado
        );
    }
}