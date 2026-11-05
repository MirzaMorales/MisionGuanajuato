using System;
using System.Collections;
using UnityEngine;

// Jefe final: Mr. Johnson (HU16). Duerme hasta que el jugador entra a su arena; entonces lanza tandas de billetes
// (bajos que se saltan y altos que se esquivan caminando) mientras se mueve de un lado a otro de su lugar.
// El jugador lo vence lanzándole comida (LanzadorComida): cada fresa, charamusca o nieve se la come y engorda.
// Con 6 bocados cae de gordo y se activa el final (JefeDerrotado). Tocarlo o a sus billetes quita una vida (tag Enemigo)
[RequireComponent(typeof(SpriteRenderer), typeof(Rigidbody2D), typeof(Collider2D))]
public class BossController : MonoBehaviour, IAplastable
{
    [Header("Cuadros (todos miran a la izquierda)")]
    [SerializeField] private Sprite cuadroParado;
    [SerializeField] private Sprite[] cuadrosCaminar;
    [SerializeField] private Sprite cuadroLanzar;
    [SerializeField] private Sprite cuadroComiendo;
    [SerializeField] private Sprite cuadroGordo;
    [SerializeField] private Sprite cuadroCaido;
    [SerializeField] private float cuadrosPorSegundo = 4f;

    [Header("Billetes")]
    [SerializeField] private Sprite[] cuadrosBillete;
    [SerializeField] private float velocidadBillete = 5.5f;
    [SerializeField] private int billetesPorTanda = 3;
    [SerializeField] private float segundosEntreBilletes = 0.7f;

    [Header("Arena")]
    // Posiciones X entre las que se pasea el jefe
    [SerializeField] private float limiteIzquierdo = 40f;
    [SerializeField] private float limiteDerecho = 46f;
    [SerializeField] private float distanciaActivacion = 14f;
    // Pausa tras despertar, para que pase el letrero de entrada antes de que empiece a lanzar billetes
    [SerializeField] private float segundosAntesDeEmpezar = 4f;
    [SerializeField] private float velocidadCaminar = 1.4f;

    [Header("Combate")]
    [SerializeField] private int comidasParaDerrotarlo = 6;
    // Cuánto se ensancha (en proporción de su ancho inicial) con cada bocado; el alto no cambia
    [SerializeField] private float crecimientoPorBocado = 0.07f;
    [SerializeField] private int puntosPorBocado = 100;
    [SerializeField] private int puntosAlDerrotarlo = 500;

    [Header("Al empezar y al caer")]
    // Ej. paredes invisibles que encierran al jugador en la arena
    [SerializeField] private GameObject[] activarAlIniciar;
    [SerializeField] private GameObject[] activarAlDerrotar;
    [SerializeField] private GameObject[] desactivarAlDerrotar;

    private enum Estado { Dormido, Peleando, Comiendo, Derrotado, Triunfo }

    // El final del nivel (FinalRescate) se suscribe aquí
    public static event Action JefeDerrotado;

    // Verdadero desde que el jugador le da el último bocado: el final feliz ya está decidido y el jugador ya no puede perder.
    // Así nunca se juntan el final feliz y el triste
    public static bool JefeVencido { get; private set; }

    private Estado estado = Estado.Dormido;
    private SpriteRenderer sr;
    private Rigidbody2D rb;
    private Collider2D col;
    private Transform jugador;
    private PlayerHealth vidasJugador;
    private Vector3 escalaInicial;
    private int bocados;
    private float direccionPaseo = -1f;
    private float relojCuadro;
    private int cuadro;
    private bool lanzando;
    private bool comiendo;

    public bool Derrotado => estado == Estado.Derrotado;

    // Si el jugador pierde, el jefe deja de pelear y la escena de derrota (FinalDerrota) lo mueve a mano
    public void Triunfar()
    {
        StopAllCoroutines();
        estado = Estado.Triunfo;
        lanzando = false;
        comiendo = false;
        rb.linearVelocity = Vector2.zero;
        rb.simulated = false;
        sr.color = Color.white;
        HUDJuego.OcultarJefe();
        foreach (BilleteProyectil b in FindObjectsByType<BilleteProyectil>(FindObjectsSortMode.None)) Destroy(b.gameObject);
    }


    // Solo es inofensivo una vez caído
    public bool EstaAplastada => estado == Estado.Derrotado || estado == Estado.Triunfo;

    public bool EsPisadaPor(Collider2D jugadorCol) => false;

    void Awake()
    {
        JefeVencido = false;
        sr = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        escalaInicial = transform.localScale;
        if (cuadroParado != null) sr.sprite = cuadroParado;
    }

    void Start()
    {
        PlayerController pc = FindFirstObjectByType<PlayerController>();
        if (pc != null) jugador = pc.transform;
        vidasJugador = FindFirstObjectByType<PlayerHealth>();
        StartCoroutine(Rutina());
    }

    void OnDestroy()
    {
        JefeDerrotado = null;
        // Al salir de la escena del jefe el jugador vuelve a poder recibir daño en cualquier otra escena
        JefeVencido = false;
    }

    // Aunque esté desactivado "Reload Domain", cada partida empieza sin jefe vencido
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ReiniciarAlIniciar()
    {
        JefeVencido = false;
        JefeDerrotado = null;
    }

    void Update()
    {
        if (estado != Estado.Peleando || lanzando) return;

        // Mira al jugador y camina entre sus límites con su ciclo de caminar
        if (jugador != null) sr.flipX = jugador.position.x > transform.position.x;

        if (cuadrosCaminar != null && cuadrosCaminar.Length > 0)
        {
            relojCuadro += Time.deltaTime;
            if (relojCuadro >= 1f / cuadrosPorSegundo)
            {
                relojCuadro = 0f;
                cuadro = (cuadro + 1) % cuadrosCaminar.Length;
                sr.sprite = cuadrosCaminar[cuadro];
            }
        }
    }

    void FixedUpdate()
    {
        if (estado != Estado.Peleando || lanzando)
        {
            if (estado != Estado.Derrotado && estado != Estado.Triunfo) rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            return;
        }

        float x = rb.position.x;
        if (x <= limiteIzquierdo) direccionPaseo = 1f;
        else if (x >= limiteDerecho) direccionPaseo = -1f;
        rb.linearVelocity = new Vector2(direccionPaseo * velocidadCaminar, rb.linearVelocity.y);
    }

    // ---------- Patrón del jefe ----------

    private IEnumerator Rutina()
    {
        // Duerme hasta que el jugador se acerca
        while (jugador == null || Mathf.Abs(jugador.position.x - transform.position.x) > distanciaActivacion) yield return null;
        foreach (GameObject o in activarAlIniciar) if (o != null) o.SetActive(true);
        yield return new WaitForSeconds(segundosAntesDeEmpezar);
        Sonido.Efecto("golpe");
        HUDJuego.Jefe("Mr. Johnson", 0, comidasParaDerrotarlo);
        estado = Estado.Peleando;

        while (estado != Estado.Derrotado)
        {
            // Pasea un rato y luego lanza una tanda de billetes; cada bocado lo hace más rápido
            float rabia = 1f + 0.12f * bocados;
            yield return new WaitForSeconds(1.6f / rabia);
            while (comiendo) yield return null;

            lanzando = true;
            if (cuadroLanzar != null) sr.sprite = cuadroLanzar;
            yield return new WaitForSeconds(0.4f / rabia);

            int cantidad = billetesPorTanda + bocados / 2;
            for (int i = 0; i < cantidad && estado != Estado.Derrotado; i++)
            {
                // Billetes bajos (se saltan) y altos (se pasan caminando) en orden aleatorio
                LanzarBillete(UnityEngine.Random.value < 0.5f ? 0.45f : 1.35f);
                yield return new WaitForSeconds(segundosEntreBilletes / rabia);
            }
            lanzando = false;
        }
    }

    // ---------- Comida lanzada por el jugador ----------

    // La llama ComidaProyectil cuando la comida le pega
    public void Comer()
    {
        if (estado != Estado.Peleando && estado != Estado.Comiendo) return;
        // Si el jugador ya se quedó sin vidas ya está decidido el final triste: nada de bocados
        if (vidasJugador != null && vidasJugador.EstaMuerto) return;
        StartCoroutine(Bocado());
    }

    private IEnumerator Bocado()
    {
        comiendo = true;
        bocados++;
        Puntuacion.Sumar(puntosPorBocado);
        Sonido.Efecto("comer");
        HUDJuego.Jefe("Mr. Johnson", bocados, comidasParaDerrotarlo);
        Chispas.Explotar(transform.position + Vector3.up * 1.5f, new Color(1f, 0.6f, 0.7f), 10, 3f);

        // Engorda con cada bocado: solo se ensancha, nunca crece de alto. Al pasar al cuadro panzón ya viene más ancho en el dibujo,
        // así que el ensanche extra es pequeño
        bool yaPanzon = bocados >= comidasParaDerrotarlo - 2 && cuadroGordo != null;
        float ancho = yaPanzon ? 1f + 0.03f * (bocados - (comidasParaDerrotarlo - 3)) : 1f + crecimientoPorBocado * bocados;
        transform.localScale = new Vector3(escalaInicial.x * ancho, escalaInicial.y, escalaInicial.z);

        if (bocados >= comidasParaDerrotarlo)
        {
            JefeVencido = true;
            foreach (BilleteProyectil b in FindObjectsByType<BilleteProyectil>(FindObjectsSortMode.None)) Destroy(b.gameObject);
            yield return Caer();
            yield break;
        }

        estado = Estado.Comiendo;
        if (cuadroComiendo != null) sr.sprite = cuadroComiendo;
        yield return new WaitForSeconds(0.6f);

        // Ya gordo, se queda con su cuadro de panzón; si no, vuelve al normal
        if (estado == Estado.Comiendo)
        {
            estado = Estado.Peleando;
            Sprite despues = bocados >= comidasParaDerrotarlo - 2 && cuadroGordo != null ? cuadroGordo : cuadroParado;
            if (despues != null) sr.sprite = despues;
        }
        comiendo = false;
    }

    private IEnumerator Caer()
    {
        estado = Estado.Derrotado;
        lanzando = false;
        rb.linearVelocity = Vector2.zero;
        col.enabled = false;
        rb.simulated = false;
        if (cuadroGordo != null) sr.sprite = cuadroGordo;
        Sonido.Efecto("golpe");
        yield return new WaitForSeconds(0.45f);

        // Se va de espaldas como un árbol, girando sobre sus pies y cada vez más rápido
        float signo = sr.flipX ? 1f : -1f;
        const float duracion = 0.7f;
        for (float t = 0f; t < duracion; t += Time.deltaTime)
        {
            float k = t / duracion;
            transform.rotation = Quaternion.Euler(0f, 0f, signo * 90f * k * k);
            yield return null;
        }

        // Cae al suelo: se cambia al cuadro acostado y rebota un poquito con una nube de polvo
        transform.rotation = Quaternion.identity;
        if (cuadroCaido != null) sr.sprite = cuadroCaido;
        Sonido.Efecto("caja");
        Chispas.Explotar(transform.position + Vector3.up * 0.15f, new Color(0.85f, 0.8f, 0.7f), 18, 4f);
        Vector3 reposo = transform.position;
        for (float t = 0f; t < 0.25f; t += Time.deltaTime)
        {
            transform.position = reposo + Vector3.up * Mathf.Sin(t / 0.25f * Mathf.PI) * 0.18f;
            yield return null;
        }
        transform.position = reposo;
        yield return new WaitForSeconds(0.4f);

        Puntuacion.Sumar(puntosAlDerrotarlo);
        Sonido.Efecto("victoria");
        HUDJuego.OcultarJefe();
        foreach (GameObject o in activarAlDerrotar) if (o != null) o.SetActive(true);
        foreach (GameObject o in desactivarAlDerrotar) if (o != null) o.SetActive(false);
        JefeDerrotado?.Invoke();
    }

    // ---------- Billetes ----------

    private void LanzarBillete(float altura)
    {
        if (cuadrosBillete == null || cuadrosBillete.Length == 0) return;

        float direccion = jugador != null && jugador.position.x > transform.position.x ? 1f : -1f;
        sr.flipX = direccion > 0f;

        GameObject go = new GameObject("Billete");
        go.tag = "Enemigo";
        go.transform.position = transform.position + new Vector3(direccion * 0.9f * transform.localScale.x, altura, 0f);

        SpriteRenderer billete = go.AddComponent<SpriteRenderer>();
        billete.sprite = cuadrosBillete[0];
        billete.sortingOrder = sr.sortingOrder + 1;

        CircleCollider2D circulo = go.AddComponent<CircleCollider2D>();
        circulo.isTrigger = true;
        circulo.radius = 0.25f;

        Rigidbody2D cuerpo = go.AddComponent<Rigidbody2D>();
        cuerpo.bodyType = RigidbodyType2D.Kinematic;

        go.AddComponent<BilleteProyectil>().Iniciar(new Vector2(direccion * velocidadBillete, 0f), cuadrosBillete, 12f, 4f);
        Sonido.Efecto("billete", 0.7f);
    }
}
