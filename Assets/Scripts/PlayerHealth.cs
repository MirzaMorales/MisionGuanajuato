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

    // "Continuar" desde el menú: recupera las vidas y el tamaño de la partida guardada
    public static void EstablecerPartida(int vidas, bool grande)
    {
        vidasGuardadas = Mathf.Max(1, vidas);
        grandeGuardado = grande;
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
        Sonido.Efecto("crecer");
        Chispas.Explotar(transform.position, new Color(1f, 0.35f, 0.4f));
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
        Sonido.Efecto("crecer");
        Chispas.Explotar(transform.position, new Color(1f, 0.8f, 0.2f), 16);
    }

    private void Encoger()
    {
        estaGrande = false;
        grandeGuardado = false;
        transform.localScale = escalaBase;
        Sonido.Efecto("encoger");
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
        Sonido.Efecto("golpe");
        Chispas.Explotar(transform.position, new Color(0.95f, 0.2f, 0.25f));

        if (EstaMuerto)
        {
            Debug.Log("Game Over");
            StopAllCoroutines();
            // La animación avisa del Game Over (evento GameOver) cuando termina
            StartCoroutine(AnimacionDerrota());
        }
        else
        {
            reaparicionPendiente = true;
            finInvulnerable = Time.time + duracionInvulnerable;
            StopAllCoroutines();
            StartCoroutine(Parpadear());
        }
    }

    // Al perder la última vida (estilo Mario): voltea a la cámara, brinca, cae girando y queda acostado en la banqueta
    // con estrellitas dando vueltas sobre la cabeza. Usa sus propios cuadros (de frente y con los ojos cerrados, este ya
    // girado), copiados en Resources/Personajes para no tener que asignarlos en cada escena.
    // Al terminar avisa del Game Over; va en finally para que la pantalla de reinicio salga aunque algo falle
    private IEnumerator AnimacionDerrota()
    {
        try
        {
            PlayerController control = GetComponent<PlayerController>();
            if (control != null) control.enabled = false;
            Animator animator = GetComponent<Animator>();
            if (animator != null) animator.enabled = false;
            sr.enabled = true;
            sr.flipX = false;

            Sprite[] cuadros = Resources.LoadAll<Sprite>("Personajes/Estudiante_Derrota");
            Array.Sort(cuadros, (a, b) => string.CompareOrdinal(a.name, b.name));
            if (cuadros.Length > 0) sr.sprite = cuadros[0];

            // Sin física: el brinco se anima a mano para poder girarlo sin que el colisionador estorbe
            Bounds caja = colJugador.bounds;
            rb.linearVelocity = Vector2.zero;
            rb.simulated = false;
            colJugador.enabled = false;

            // Piso sólido que tenga debajo (aunque el golpe lo agarre en el aire); sin piso es que cayó a un hueco
            bool enPiso = false;
            float piso = caja.min.y;
            foreach (RaycastHit2D golpe in Physics2D.RaycastAll(new Vector2(caja.center.x, caja.min.y + 0.05f), Vector2.down, 6f))
            {
                if (golpe.collider.isTrigger) continue;
                enPiso = true;
                piso = golpe.point.y;
                break;
            }

            // 1) Golpe: congelado de frente
            yield return new WaitForSeconds(0.4f);

            // 2) Brinco; al bajar gira hasta quedar acostado. Si cayó a un hueco sigue bajando hasta salir de la pantalla
            Vector3 inicio = transform.position;
            const float altura = 1.4f, duracion = 0.8f;
            // Al final del brinco los pies llegan al piso encontrado (o bajan 8 unidades si no hay)
            float bajada = enPiso ? piso - caja.min.y : -8f;
            for (float t = 0f; t < duracion; t += Time.deltaTime)
            {
                float k = t / duracion;
                float y = 4f * altura * k * (1f - k) + bajada * k * k;
                transform.position = inicio + Vector3.up * y;
                transform.rotation = Quaternion.Euler(0f, 0f, 90f * Mathf.Clamp01(k * 2f - 1f));
                yield return null;
            }
            if (!enPiso) yield break;

            // 3) Acostado sobre la banqueta con los ojos cerrados (el cuadro ya viene girado, con el pivote en la base)
            transform.rotation = Quaternion.identity;
            if (cuadros.Length > 1) sr.sprite = cuadros[1];
            transform.position = new Vector3(inicio.x, piso, inicio.z);
            Sonido.Efecto("golpe");

            // 4) Estrellitas girando sobre la cabeza (que quedó a la izquierda)
            Bounds cuerpo = sr.bounds;
            CrearEstrellitas(new Vector3(cuerpo.min.x + cuerpo.size.x * 0.25f, cuerpo.max.y + 0.12f, 0f));
            yield return new WaitForSeconds(1.0f);
        }
        finally
        {
            GameOver?.Invoke();
        }
    }

    // Tres estrellitas que giran en óvalo; usan tiempo real para seguir girando detrás de la pantalla de Game Over
    private void CrearEstrellitas(Vector3 centro)
    {
        Sprite estrella = Resources.Load<Sprite>("Personajes/Estrella_Mareo");
        if (estrella == null) return;
        GameObject grupo = new GameObject("Estrellitas");
        grupo.transform.position = centro;
        for (int i = 0; i < 3; i++)
        {
            GameObject go = new GameObject("Estrella");
            go.transform.SetParent(grupo.transform, false);
            SpriteRenderer e = go.AddComponent<SpriteRenderer>();
            e.sprite = estrella;
            e.sortingOrder = sr.sortingOrder + 1;
        }
        grupo.AddComponent<EstrellitasMareo>();
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
        // El HUD (HUDJuego) ya muestra las vidas con corazones; este texto solo queda para escenas de prueba
        if (!mostrarVidasEnPantalla || HUDJuego.Activo) return;
        GUI.Label(new Rect(10, 10, 200, 25), $"Vidas: {vidasActuales}" + (estaGrande ? "  (grande)" : ""));
    }
}
