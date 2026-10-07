using System;
using System.Collections;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private int vidasMaximas = 3;

    [Header("Depuración")]
    [SerializeField] private bool mostrarVidasEnPantalla = true;

    [Header("Invulnerabilidad tras el golpe")]
    [SerializeField] private float duracionInvulnerable = 1.5f;
    [SerializeField] private float intervaloParpadeo = 0.1f;

    private int vidasActuales;
    private float finInvulnerable;
    private SpriteRenderer sr;
    private Rigidbody2D rb;
    private Vector3 puntoReaparicion;
    private bool reaparicionPendiente;

    // Otros sistemas (HUD, checkpoint, pantalla de Game Over) se suscriben a estos eventos
    public event Action<int> VidasCambiadas;
    public event Action GameOver;

    public int VidasActuales => vidasActuales;
    public bool EstaMuerto => vidasActuales <= 0;
    public bool EsInvulnerable => Time.time < finInvulnerable;

    void Awake()
    {
        vidasActuales = vidasMaximas;
        sr = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        // Mientras no se active ningún checkpoint se reaparece donde empezó el nivel
        puntoReaparicion = transform.position;
    }

    void Start()
    {
        // Start y no Awake: así los demás scripts ya alcanzaron a suscribirse
        VidasCambiadas?.Invoke(vidasActuales);
    }

    // Lo llama Checkpoint.cs al tocarlo; el último activado reemplaza al anterior
    public void EstablecerPuntoReaparicion(Vector3 posicion)
    {
        puntoReaparicion = posicion;
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
            RecibirDano();
        }
    }

    [ContextMenu("Probar: recibir daño")]
    public void RecibirDano()
    {
        if (EstaMuerto || EsInvulnerable) return;

        vidasActuales--;
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
        GUI.Label(new Rect(10, 10, 200, 25), $"Vidas: {vidasActuales}");
    }
}
