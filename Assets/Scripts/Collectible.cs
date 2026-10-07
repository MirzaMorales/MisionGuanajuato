using UnityEngine;

// Requiere un Collider2D con "Is Trigger" activado
public class Collectible : MonoBehaviour
{
    [SerializeField] private int puntos = 10;
    [SerializeField] private AudioClip sonidoRecoleccion;
    [SerializeField, Range(0f, 1f)] private float volumen = 1f;

    private bool recogido;

    void OnTriggerEnter2D(Collider2D col)
    {
        if (recogido) return;
        // Solo el jugador lo recoge (el Estudiante no tiene tag, se reconoce por su controlador)
        if (col.GetComponent<PlayerController>() == null) return;

        recogido = true;
        Puntuacion.Sumar(puntos);

        // PlayClipAtPoint sigue sonando aunque este objeto se destruya enseguida
        if (sonidoRecoleccion != null)
        {
            AudioSource.PlayClipAtPoint(sonidoRecoleccion, transform.position, volumen);
        }

        Destroy(gameObject);
    }
}
