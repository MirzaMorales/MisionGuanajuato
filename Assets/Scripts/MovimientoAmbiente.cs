using UnityEngine;

// Movimiento suave de adorno para objetos de un solo cuadro (burros, mojigangas...): se mecen alrededor de sus pies y,
// si se pide, dan un pequeño brinco. No cambia el colisionador de forma notoria porque los ángulos son pequeños
public class MovimientoAmbiente : MonoBehaviour
{
    // Grados máximos que se inclina a cada lado
    [SerializeField] private float anguloMaximo = 3f;
    // Vueltas del vaivén por segundo
    [SerializeField] private float velocidad = 0.6f;
    // Altura (en unidades) del brinquito; 0 = sin brinco
    [SerializeField] private float rebote = 0f;

    private Vector3 posicionBase;
    private float fase;

    void Start()
    {
        posicionBase = transform.position;
        // Cada objeto arranca en un momento distinto para que no se muevan todos al mismo tiempo
        fase = Random.value * Mathf.PI * 2f;
    }

    void Update()
    {
        float t = Time.time * velocidad * Mathf.PI * 2f + fase;
        transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Sin(t) * anguloMaximo);
        if (rebote > 0f) transform.position = posicionBase + Vector3.up * Mathf.Abs(Mathf.Sin(t * 0.5f)) * rebote;
    }
}
