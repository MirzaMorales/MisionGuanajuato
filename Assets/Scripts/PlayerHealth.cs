using System;
using System.Collections;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private int vidasMaximas = 3;
    // Con corazones extra se puede pasar de las vidas iniciales, hasta este tope
    [SerializeField] private int vidasTope = 5;

    [Header("Depuración")]
    [SerializeField] private bool mostrarVidasEnPantalla = true;

    [Header("Invulnerabilidad tras el golpe")]
    [SerializeField] private float duracionInvulnerable = 1.5f;
    [SerializeField] private float intervaloParpadeo = 0.1f;

    [Header("Crecimiento (envase de mermelada)")]
    [SerializeField] private float factorCrecimiento = 1.3f;

    // Las vidas y el crecimiento se conservan entre escenas y niveles; -1 significa "partida nueva"
    private static int vidasGuardadas = -1;
    private static bool grandeGuardado;

    private int vidasActuales;
    private bool estaGrande;
    private Vector3 escalaBase;
    private float finInvulnerable;
    private SpriteRenderer sr;
    private Rigidbody2D rb;
    private Collider2D colJugador;
    private Vector3 puntoReaparicion;
    private bool reaparicionPendiente;

    // Otros sistemas (HUD, checkpoint, pantalla de Game Over) se suscriben a estos eventos
    public event Action<int> VidasCambiadas;
    public event Action GameOver;

    public int VidasActuales => vidasActuales;
    public bool EstaMuerto => vidasActuales <= 0;
    public bool EsInvulnerable => Time.time < finInvulnerable;
    public bool EstaGrande => estaGrande;

    void Awake()
    {
        // Si las vidas guardadas son 0 (Game Over) también se empieza con las máximas y sin crecer
        bool partidaNueva = vidasGuardadas <= 0;
        vidasActuales = partidaNueva ? vidasMaximas : vidasGuardadas;
        if (partidaNueva) grandeGuardado = false;

        sr = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        colJugador = GetComponent<Collider2D>();
        escalaBase = transform.localScale;
        if (grandeGuardado)
        {
            estaGrande = true;
            transform.localScale = escalaBase * factorCrecimiento;
        }
        // Mientras no se active ningún checkpoint se reaparece donde empezó el nivel
        puntoReaparicion = transform.position;
    }

    void Start()
    {
        // Start y no Awake: así los demás scripts ya alcanzaron a suscribirse
        VidasCambiadas?.Invoke(vidasActuales);
    }

    // La pantalla de Game Over (HU19) o el menú de inicio la llaman para empezar una partida con las vidas completas
    public static void ReiniciarVidas()
    {
        vidasGuardadas = -1;
        grandeGuardado = false;
    }

    // Se ejecuta al entrar a Play, aunque esté desactivado "Reload Domain", para que cada partida empiece con las vidas completas
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ReiniciarAlIniciarPartida()
    {
        vidasGuardadas = -1;
        grandeGuardado = false;
    }

    // Lo llama Checkpoint.cs al tocarlo; el último activado reemplaza al anterior
    public void EstablecerPuntoReaparicion(Vector3 posicion)
    {
        puntoReaparicion = posicion;
    }

    // Lo llama el corazón (ItemVida): suma vidas hasta el tope
    public void GanarVida(int cantidad = 1)
    {
        if (EstaMuerto) return;

        vidasActuales = Mathf.Min(vidasActuales + cantidad, vidasTope);
        vidasGuardadas = vidasActuales;
        Debug.Log($"Vida extra: {vidasActuales}");
        VidasCambiadas?.Invoke(vidasActuales);
    }

    // Lo llama el envase de mermelada (ItemCrecimiento): el Estudiante crece y el siguiente golpe solo lo encoge
    public void Crecer()
    {
        if (estaGrande) return;

        estaGrande = true;
        grandeGuardado = true;
        transform.localScale = escalaBase * factorCrecimiento;
        // El collider crece hacia abajo desde el centro: se sube un poco para no clavarlo en el piso
        transform.position += Vector3.up * 0.2f;
    }

    private void Encoger()
    {
        estaGrande = false;
        grandeGuardado = false;
        transform.localScale = escalaBase;
    }

    // El teletransporte se hace en FixedUpdate y no dentro del callback de colisión,
    // porque mover el Rigidbody2D en pleno paso de física puede ignorarse
    void FixedUpdate()
    {
        if (!reaparicionPendiente) return;

        reaparicionPendiente = false;
        Debug.Log($"Reaparece en {puntoReaparicion} (venía de {transform.position})");
        rb.position = puntoReaparicion;
        transform.position = puntoReaparicion;
        rb.linearVelocity = Vector2.zero;
    }

    // Stay además de Enter: si el jugador sigue pegado al peligro cuando termina la invulnerabilidad, vuelve a recibir daño
    void OnCollisionEnter2D(Collision2D col) => IntentarDano(col.gameObject);
    void OnCollisionStay2D(Collision2D col) => IntentarDano(col.gameObject);
    void OnTriggerEnter2D(Collider2D col) => IntentarDano(col.gameObject);
    void OnTriggerStay2D(Collider2D col) => IntentarDano(col.gameObject);

    private void IntentarDano(GameObject otro)
    {
        if (otro.CompareTag("Enemigo") || otro.CompareTag("Obstaculo"))
        {
            // A una rata se le puede caer encima para aplastarla: eso no hace daño
            Rata rata = otro.GetComponent<Rata>();
            if (rata != null && (rata.EstaAplastada || rata.EsPisadaPor(colJugador))) return;

            // Una caída al vacío (ZonaLetal) quita una vida aunque el Estudiante esté grande
            RecibirDano(otro.GetComponent<ZonaLetal>() != null);
        }
    }

    [ContextMenu("Probar: recibir daño")]
    public void RecibirDano()
    {
        RecibirDano(false);
    }

    public void RecibirDano(bool letal)
    {
        if (EstaMuerto || EsInvulnerable) return;

        // Grande: el tamaño funciona como una vida extra. El golpe solo lo devuelve a tamaño normal y no cuesta una vida.
        // Si el golpe es una caída al vacío (letal) además reaparece en el checkpoint, porque no puede quedarse en el hueco
        if (estaGrande)
        {
            Encoger();
            finInvulnerable = Time.time + duracionInvulnerable;
            if (letal) reaparicionPendiente = true;
            StopAllCoroutines();
            StartCoroutine(Parpadear());
            return;
        }

        vidasActuales--;
        vidasGuardadas = vidasActuales;
        Debug.Log($"Vidas restantes: {vidasActuales}");
        VidasCambiadas?.Invoke(vidasActuales);

        if (EstaMuerto)
        {
            Debug.Log("Game Over");
            GameOver?.Invoke();
        }
        else
        {
            reaparicionPendiente = true;
            finInvulnerable = Time.time + duracionInvulnerable;
            StopAllCoroutines();
            StartCoroutine(Parpadear());
        }
    }

    private IEnumerator Parpadear()
    {
        while (EsInvulnerable)
        {
            sr.enabled = !sr.enabled;
            yield return new WaitForSeconds(intervaloParpadeo);
        }
        sr.enabled = true;
    }

    // Contador temporal de vidas para pruebas, hasta que exista el HUD de vidas
    void OnGUI()
    {
        if (!mostrarVidasEnPantalla) return;
        GUI.Label(new Rect(10, 10, 200, 25), $"Vidas: {vidasActuales}" + (estaGrande ? "  (grande)" : ""));
    }
}
