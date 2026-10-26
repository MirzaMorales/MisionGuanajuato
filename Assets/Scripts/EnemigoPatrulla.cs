using UnityEngine;

// Enemigo que patrulla (reloj despertador, examen...). Espera quieto hasta que el jugador se acerca; entonces camina hacia él
// y va de un lado a otro: da vuelta al chocar con algo y no se cae de las orillas (huecos).
// Si el jugador le cae encima lo aplasta (rebote y puntos); si lo toca de lado, PlayerHealth le quita una vida (tag Enemigo)
[RequireComponent(typeof(SpriteRenderer), typeof(Rigidbody2D), typeof(Collider2D))]
public class EnemigoPatrulla : MonoBehaviour
{
    [Header("Cuadros (todos miran a la izquierda)")]
    [SerializeField] private Sprite[] cuadrosCaminar;
    [SerializeField] private Sprite cuadroParado;
    [SerializeField] private Sprite cuadroAplastado;
    [SerializeField] private float cuadrosPorSegundo = 8f;

    [Header("Comportamiento")]
    // Distancia en X a la que el enemigo despierta y empieza a caminar
    [SerializeField] private float distanciaActivacion = 9f;
    [SerializeField] private float velocidad = 1.3f;
    [SerializeField] private LayerMask capaSuelo;
    [SerializeField] private int puntosAlAplastar = 20;
    [SerializeField] private float reboteJugador = 9f;
    [SerializeField] private string sonidoAplastar = "aplastar";
    [SerializeField] private Color colorChispas = new Color(0.8f, 0.6f, 0.3f);

    private enum Estado { Esperando, Caminando, Aplastado }

    private Estado estado = Estado.Esperando;
    private SpriteRenderer sr;
    private Rigidbody2D rb;
    private Collider2D col;
    private Transform jugador;
    private float direccion = -1f;
    private float relojCuadro;
    private int cuadro;
    private float finBloqueoGiro;

    public bool EstaAplastado => estado == Estado.Aplastado;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        if (cuadroParado != null) sr.sprite = cuadroParado;
    }

    void Start()
    {
        PlayerController pc = FindFirstObjectByType<PlayerController>();
        if (pc != null) jugador = pc.transform;
    }

    void Update()
    {
        if (estado == Estado.Esperando)
        {
            if (jugador != null && Mathf.Abs(jugador.position.x - transform.position.x) < distanciaActivacion)
            {
                estado = Estado.Caminando;
                direccion = Mathf.Sign(jugador.position.x - transform.position.x);
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
            if (EsPisadoPor(otro.collider)) Aplastar(otro.rigidbody);
            return;
        }

        // Chocó de lado con algo (obstáculo, pared, otro enemigo): da la vuelta
        foreach (ContactPoint2D contacto in otro.contacts)
        {
            if (Mathf.Abs(contacto.normal.x) > 0.7f)
            {
                Girar();
                return;
            }
        }
    }

    // El jugador le cayó encima: sus pies están a la altura de la cabeza del enemigo o más arriba
    public bool EsPisadoPor(Collider2D jugadorCol)
    {
        return estado == Estado.Caminando && jugadorCol != null && jugadorCol.bounds.min.y >= col.bounds.max.y - 0.25f;
    }

    private void Aplastar(Rigidbody2D rbJugador)
    {
        estado = Estado.Aplastado;
        if (cuadroAplastado != null) sr.sprite = cuadroAplastado;
        col.enabled = false;
        rb.linearVelocity = Vector2.zero;
        rb.simulated = false;
        Puntuacion.Sumar(puntosAlAplastar);
        Sonido.Efecto(sonidoAplastar);
        Chispas.Explotar(transform.position + Vector3.up * 0.3f, colorChispas, 12);

        if (rbJugador != null) rbJugador.linearVelocity = new Vector2(rbJugador.linearVelocity.x, reboteJugador);
        Destroy(gameObject, 0.6f);
    }
}
