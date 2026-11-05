using System.Collections.Generic;
using UnityEngine;

// Plataforma que va y viene entre su posición inicial y otra (posición inicial + recorrido).
// Sirve para los elevadores de mina (recorrido vertical, con pausa en cada extremo) y para las
// carretillas que ruedan por el riel (recorrido horizontal, sin pausa y volteando el dibujo).
// Lleva encima al Estudiante: lo que esté parado sobre ella se mueve con ella.
// Requiere un Rigidbody2D Kinematic y un Collider2D sólido en la capa Suelo (para poder saltar desde ella).
// Con "lastimaDeLado" (carretilla) quita una vida si embiste al Estudiante de lado; pararse encima no lastima.
// Si al bajar aplasta al Estudiante contra el piso, también le quita una vida
[RequireComponent(typeof(Rigidbody2D))]
public class PlataformaMovil : MonoBehaviour
{
    [SerializeField] private Vector2 recorrido = new Vector2(0f, 3f);
    [SerializeField] private float velocidad = 1.5f;
    [SerializeField] private float pausaEnExtremos = 0.8f;
    // Para las carretillas: el dibujo mira hacia donde avanza
    [SerializeField] private bool voltearAlAvanzar;
    [SerializeField] private bool lastimaDeLado;
    // Empieza en un punto intermedio del recorrido (0 = inicio, 1 = final) para que dos plataformas no vayan igual
    [SerializeField, Range(0f, 1f)] private float desfase;

    private Rigidbody2D rb;
    private Collider2D col;
    private SpriteRenderer sr;
    private Vector2 inicio;
    private float avance;
    private int sentido = 1;
    private float finPausa;
    private Vector2 ultimoMovimiento;
    private readonly HashSet<Rigidbody2D> pasajeros = new HashSet<Rigidbody2D>();

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        sr = GetComponent<SpriteRenderer>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        inicio = rb.position;
        avance = desfase * recorrido.magnitude;
        rb.position = PuntoActual();
    }

    void FixedUpdate()
    {
        ultimoMovimiento = Vector2.zero;
        if (Time.time < finPausa) return;

        float largo = recorrido.magnitude;
        if (largo < 0.01f) return;

        avance += sentido * velocidad * Time.fixedDeltaTime;
        if (avance >= largo || avance <= 0f)
        {
            avance = Mathf.Clamp(avance, 0f, largo);
            sentido = -sentido;
            finPausa = Time.time + pausaEnExtremos;
        }

        Vector2 anterior = rb.position;
        Vector2 nueva = PuntoActual();
        rb.MovePosition(nueva);

        // Los pasajeros avanzan de lado lo mismo que la plataforma y bajan con ella. Al subir no se les mueve:
        // la plataforma ya los empuja con la física, y moverlos además los hacía brincar (doble empuje)
        Vector2 delta = nueva - anterior;
        ultimoMovimiento = delta;
        Vector2 arrastre = new Vector2(delta.x, Mathf.Min(delta.y, 0f));
        foreach (Rigidbody2D p in pasajeros)
        {
            // Si acaba de saltar ya no viaja con la plataforma
            if (p == null || p.linearVelocity.y > 0.5f) continue;
            p.position += arrastre;
            // Sin velocidad vertical propia mientras va encima, para que no rebote al cambiar de sentido
            if (delta.y != 0f && p.linearVelocity.y < 0f) p.linearVelocity = new Vector2(p.linearVelocity.x, 0f);
        }

        if (voltearAlAvanzar && sr != null && Mathf.Abs(recorrido.x) > 0.01f)
        {
            sr.flipX = sentido * Mathf.Sign(recorrido.x) < 0f;
        }
    }

    private Vector2 PuntoActual()
    {
        return inicio + recorrido.normalized * avance;
    }

    void OnCollisionEnter2D(Collision2D otro) => RevisarContacto(otro);
    void OnCollisionStay2D(Collision2D otro) => RevisarContacto(otro);

    private void RevisarContacto(Collision2D otro)
    {
        if (otro.rigidbody == null || otro.rigidbody.bodyType != RigidbodyType2D.Dynamic) return;
        // Solo cuenta si está parado encima (sus pies a la altura de la superficie)
        if (otro.collider.bounds.min.y >= col.bounds.max.y - 0.2f)
        {
            pasajeros.Add(otro.rigidbody);
            return;
        }
        pasajeros.Remove(otro.rigidbody);

        PlayerHealth jugador = otro.collider.GetComponent<PlayerHealth>();
        if (jugador == null) return;

        // Aplastado: la plataforma baja, el Estudiante está debajo de ella y tiene piso debajo de los pies
        bool debajo = otro.collider.bounds.max.y <= col.bounds.min.y + 0.25f;
        if (debajo && ultimoMovimiento.y < 0f && TienePisoDebajo(otro.collider))
        {
            jugador.RecibirDano(true);
            return;
        }

        if (lastimaDeLado && !debajo) jugador.RecibirDano();
    }

    // ¿Hay algo sólido justo debajo del Estudiante (que no sea esta plataforma)?
    private bool TienePisoDebajo(Collider2D cuerpo)
    {
        Bounds b = cuerpo.bounds;
        Vector2 origen = new Vector2(b.center.x, b.min.y + 0.02f);
        foreach (RaycastHit2D golpe in Physics2D.RaycastAll(origen, Vector2.down, 0.15f))
        {
            if (golpe.collider != cuerpo && golpe.collider != col && !golpe.collider.isTrigger) return true;
        }
        return false;
    }

    void OnCollisionExit2D(Collision2D otro)
    {
        if (otro.rigidbody != null) pasajeros.Remove(otro.rigidbody);
    }

    // Dibuja el recorrido en la pestaña Scene para acomodarla fácil
    void OnDrawGizmosSelected()
    {
        Vector3 desde = Application.isPlaying ? (Vector3)inicio : transform.position;
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(desde, desde + (Vector3)recorrido);
        Gizmos.DrawWireSphere(desde + (Vector3)recorrido, 0.2f);
    }
}
