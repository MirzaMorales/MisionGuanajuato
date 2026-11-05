using System.Collections.Generic;
using UnityEngine;

// Punto de ayuda del tutorial dentro del nivel. Es un objeto vacío colocado en el escenario;
// cuando el jugador está cerca, su mensaje aparece en el recuadro de ayudas de la parte inferior
// de la pantalla (AyudaTutorial). Así las ayudas nunca tapan el HUD ni se enciman entre sí.
// Las teclas pueden resaltarse con <color=#FFC832>...</color> dentro del mensaje
public class CartelTutorial : MonoBehaviour
{
    [SerializeField, TextArea] private string mensaje = "Salta con ESPACIO";
    [SerializeField] private float distanciaVisible = 4f;

    // Todas las ayudas de la escena (AyudaTutorial busca la más cercana al jugador)
    public static readonly List<CartelTutorial> Todos = new List<CartelTutorial>();

    public string Mensaje => mensaje;
    public float DistanciaVisible => distanciaVisible;

    void OnEnable() => Todos.Add(this);
    void OnDisable() => Todos.Remove(this);

    // Dibuja el alcance de la ayuda en la pestaña Scene para acomodarla fácilmente
    void OnDrawGizmos()
    {
        Gizmos.color = new Color(1f, 0.78f, 0.2f, 0.8f);
        Gizmos.DrawWireCube(transform.position, new Vector3(distanciaVisible * 2f, 0.4f, 0f));
        Gizmos.DrawSphere(transform.position, 0.15f);
    }
}
