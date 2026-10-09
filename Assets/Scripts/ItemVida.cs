using UnityEngine;

// Corazón: al tocarlo el jugador gana una vida. Requiere un Collider2D con "Is Trigger" activado
public class ItemVida : MonoBehaviour
{
    [SerializeField] private int vidas = 1;

    void OnTriggerEnter2D(Collider2D col)
    {
        PlayerHealth jugador = col.GetComponent<PlayerHealth>();
        if (jugador == null) return;

        jugador.GanarVida(vidas);
        Destroy(gameObject);
    }
}
