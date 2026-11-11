using UnityEngine;

// Comida que lanza el jugador (LanzadorComida). Vuela en línea recta, horizontal, hacia donde mira el Estudiante.
// - Al tocar a Mr. Johnson se la come (BossController.Comer).
// - Al tocar una rata, momia, examen, reloj o turista, lo derriba (suma sus puntos) y la comida desaparece.
// - Desaparece también al chocar con el piso o una plataforma, o pasado su tiempo
public class ComidaProyectil : MonoBehaviour
{
    private Vector2 velocidad;
    private float vidaRestante = 1.5f;

    public void Iniciar(Vector2 velocidadInicial)
    {
        velocidad = velocidadInicial;
    }

    void Update()
    {
        transform.position += (Vector3)(velocidad * Time.deltaTime);
        transform.Rotate(0f, 0f, -360f * Time.deltaTime);

        vidaRestante -= Time.deltaTime;
        if (vidaRestante <= 0f) Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D otro)
    {
        BossController jefe = otro.GetComponent<BossController>();
        if (jefe != null)
        {
            jefe.Comer();
            Destroy(gameObject);
            return;
        }

        // Enemigos que se pueden derribar (Rata, EnemigoPatrulla): la comida los golpea y se acaba
        IAplastable enemigo = otro.GetComponent<IAplastable>();
        if (enemigo != null && enemigo.RecibirProyectil())
        {
            Destroy(gameObject);
            return;
        }

        // Capa 6 = Suelo (piso, plataformas, cajas): la comida se estrella de lado
        if (otro.gameObject.layer == 6) Destroy(gameObject);
    }
}
