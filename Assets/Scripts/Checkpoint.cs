using UnityEngine;

// Requiere un Collider2D con "Is Trigger" activado. La posición del objeto es donde reaparecerá el jugador
public class Checkpoint : MonoBehaviour
{
    [SerializeField] private Color colorActivado = Color.green;

    private bool activado;

    void Awake()
    {
        if (CompareTag("Enemigo") || CompareTag("Obstaculo"))
        {
            Debug.LogWarning($"{name} es Checkpoint y además tiene tag de daño: al tocarlo el jugador reaparecería sobre él", this);
        }
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (activado) return;

        PlayerHealth jugador = col.GetComponent<PlayerHealth>();
        if (jugador == null) return;

        jugador.EstablecerPuntoReaparicion(transform.position);
        activado = true;

        // Aviso visual de que ya quedó guardado
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null) sr.color = colorActivado;
    }
}
