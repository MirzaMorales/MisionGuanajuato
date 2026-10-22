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
        else
        {
            // Sin clip asignado se usa el sonido general de recolección del juego
            Sonido.Efecto("fresa");
        }
        Chispas.Explotar(transform.position, new Color(1f, 0.3f, 0.35f), 8, 3f);

        Destroy(gameObject);
    }
}
