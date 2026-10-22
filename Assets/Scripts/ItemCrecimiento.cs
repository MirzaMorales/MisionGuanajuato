using UnityEngine;

// Envase de mermelada: al tocarlo el Estudiante crece. Requiere un Collider2D con "Is Trigger" activado
public class ItemCrecimiento : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D col)
    {
        PlayerHealth jugador = col.GetComponent<PlayerHealth>();
        if (jugador == null) return;

        jugador.Crecer();
        Destroy(gameObject);
    }
}
