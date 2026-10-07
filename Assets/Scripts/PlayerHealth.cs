using System;
using System.Collections;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private int vidasMaximas = 3;

    [Header("Invulnerabilidad tras el golpe")]
    [SerializeField] private float duracionInvulnerable = 1.5f;
    [SerializeField] private float intervaloParpadeo = 0.1f;

    private int vidasActuales;
    private float finInvulnerable;
    private SpriteRenderer sr;

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
    }

    void Start()
    {
        // Start y no Awake: así los demás scripts ya alcanzaron a suscribirse
        VidasCambiadas?.Invoke(vidasActuales);
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
        VidasCambiadas?.Invoke(vidasActuales);

        if (EstaMuerto)
        {
            Debug.Log("Game Over");
            GameOver?.Invoke();
        }
        else
        {
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
}
