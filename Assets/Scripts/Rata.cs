using System.Collections;
using UnityEngine;

// Rata de alcantarilla (enemigo tipo Goomba). Espera escondida bajo su alcantarilla; cuando el jugador se acerca
// se asoman sus ojos, se abre la tapa y la rata sube desde el hueco. Luego camina hacia el jugador y va de un lado
// a otro: da vuelta al chocar con algo y no se cae de las orillas (huecos).
// Si el jugador le cae encima la aplasta (rebote y puntos); si la toca de lado, PlayerHealth le quita una vida (tag Enemigo)
[RequireComponent(typeof(SpriteRenderer), typeof(Rigidbody2D), typeof(Collider2D))]
public class Rata : MonoBehaviour
{
    [Header("Alcantarilla")]
    [SerializeField] private SpriteRenderer alcantarilla;
    [SerializeField] private Sprite alcantarillaOjos;
    [SerializeField] private Sprite alcantarillaAbierta;

    [Header("Cuadros de la rata")]
    [SerializeField] private Sprite[] cuadrosCaminar;
    [SerializeField] private Sprite cuadroParada;
    [SerializeField] private Sprite cuadroAplastada;
    [SerializeField] private float cuadrosPorSegundo = 8f;

    [Header("Aparición")]
    [SerializeField] private float distanciaActivacion = 7f;
    [SerializeField] private float segundosOjos = 0.6f;
    [SerializeField] private float segundosSubida = 0.45f;
    // Cuánto más abajo empieza la rata (escondida detrás del piso) antes de subir
    [SerializeField] private float profundidad = 1.0f;

    [Header("Comportamiento")]
    [SerializeField] private float velocidad = 1.2f;
    [SerializeField] private LayerMask capaSuelo;
    [SerializeField] private int puntosAlAplastar = 20;
    [SerializeField] private float reboteJugador = 9f;

    private enum Estado { Escondida, Apareciendo, Caminando, Aplastada }

    private Estado estado = Estado.Escondida;
    private SpriteRenderer sr;
    private Rigidbody2D rb;
    private Collider2D col;
    private Transform jugador;
    private Vector3 posicionParada;
    private int ordenNormal;
    private float direccion = -1f;
    private float relojCuadro;
    private int cuadro;
    private float finBloqueoGiro;

    public bool EstaAplastada => estado == Estado.Aplastada;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();

        // Escondida: no se ve ni choca con nada; solo está la alcantarilla cerrada
        posicionParada = transform.position;
        ordenNormal = sr.sortingOrder;
        sr.enabled = false;
        col.enabled = false;
        rb.simulated = false;
    }

    void Start()
    {
        PlayerController pc = FindFirstObjectByType<PlayerController>();
        if (pc != null) jugador = pc.transform;
    }

    void Update()
    {
        if (estado == Estado.Escondida)
        {
            if (jugador != null && Mathf.Abs(jugador.position.x - transform.position.x) < distanciaActivacion)
            {
                StartCoroutine(Aparecer());
            }
            return;
        }

        if (estado != Estado.Caminando || cuadrosCaminar == null || cuadrosCaminar.Length == 0) return;

        // Ciclo de caminar; los cuadros miran a la izquierda, se voltean al ir a la derecha
        relojCuadro += Time.deltaTime;
        if (relojCuadro >= 1f / cuadrosPorSegundo)
        {
            relojCuadro = 0f;
            cuadro = (cuadro + 1) % cuadrosCaminar.Length;
            sr.sprite = cuadrosCaminar[cuadro];
        }
        sr.flipX = direccion > 0f;
    }

    void FixedUpdate()
    {
        if (estado != Estado.Caminando) return;

        rb.linearVelocity = new Vector2(direccion * velocidad, rb.linearVelocity.y);

        // Si adelante ya no hay piso (orilla de un hueco), da la vuelta en lugar de caerse
        Bounds b = col.bounds;
        Vector2 frente = new Vector2(b.center.x + direccion * (b.extents.x + 0.05f), b.min.y + 0.05f);
        bool hayPiso = Physics2D.Raycast(frente, Vector2.down, 0.4f, capaSuelo);
        bool enElAire = Mathf.Abs(rb.linearVelocity.y) > 0.1f;
        if (!hayPiso && !enElAire) Girar();
    }

    private IEnumerator Aparecer()
    {
        estado = Estado.Apareciendo;
        if (jugador != null) direccion = Mathf.Sign(jugador.position.x - transform.position.x);

        // 1) Se asoman los ojos bajo la tapa
        if (alcantarilla != null && alcantarillaOjos != null) alcantarilla.sprite = alcantarillaOjos;
        yield return new WaitForSeconds(segundosOjos);

        // 2) Se abre la tapa y la rata sube desde el hueco (detrás del piso, para que parezca que sale del drenaje)
        if (alcantarilla != null && alcantarillaAbierta != null) alcantarilla.sprite = alcantarillaAbierta;
        sr.sprite = cuadroParada != null ? cuadroParada : sr.sprite;
        sr.flipX = direccion > 0f;
        sr.sortingOrder = -7;
        sr.enabled = true;

        Vector3 abajo = posicionParada + Vector3.down * profundidad;
        float t = 0f;
        while (t < segundosSubida)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / segundosSubida);
            // Sube un poco más arriba de su lugar y cae (un brinquito al salir)
            float salto = Mathf.Sin(k * Mathf.PI) * 0.35f;
            transform.position = Vector3.Lerp(abajo, posicionParada, k) + Vector3.up * salto;
            yield return null;
        }
        transform.position = posicionParada;
        sr.sortingOrder = ordenNormal;

        col.enabled = true;
        rb.simulated = true;
        estado = Estado.Caminando;
    }

    private void Girar()
    {
        if (Time.time < finBloqueoGiro) return;
        direccion = -direccion;
        finBloqueoGiro = Time.time + 0.25f;
    }

    void OnCollisionEnter2D(Collision2D otro) => RevisarChoque(otro);
    void OnCollisionStay2D(Collision2D otro) => RevisarChoque(otro);

    private void RevisarChoque(Collision2D otro)
    {
        if (estado != Estado.Caminando) return;

        if (otro.collider.GetComponent<PlayerController>() != null)
        {
            if (EsPisadaPor(otro.collider)) Aplastar(otro.rigidbody);
            return;
        }

        // Chocó de lado con algo (bote, jardinera, otra rata, pared): da la vuelta
        foreach (ContactPoint2D contacto in otro.contacts)
        {
            if (Mathf.Abs(contacto.normal.x) > 0.7f)
            {
                Girar();
                return;
            }
        }
    }

    // El jugador le cayó encima: sus pies están a la altura de la cabeza de la rata o más arriba
    public bool EsPisadaPor(Collider2D jugadorCol)
    {
        return estado == Estado.Caminando && jugadorCol != null && jugadorCol.bounds.min.y >= col.bounds.max.y - 0.25f;
    }

    private void Aplastar(Rigidbody2D rbJugador)
    {
        estado = Estado.Aplastada;
        if (cuadroAplastada != null) sr.sprite = cuadroAplastada;
        col.enabled = false;
        rb.linearVelocity = Vector2.zero;
        rb.simulated = false;
        Puntuacion.Sumar(puntosAlAplastar);

        if (rbJugador != null) rbJugador.linearVelocity = new Vector2(rbJugador.linearVelocity.x, reboteJugador);
        Destroy(gameObject, 0.6f);
    }
}
