using System.Collections;
using UnityEngine;

// Final del juego (HU18): en cuanto cae Mr. Johnson, el jugador pierde el control y todo ocurre solo, como un video:
// Doña María suelta la mano de la novia, la novia corre hacia el Estudiante, él camina hacia ella y se abrazan.
// Después aparece la pantalla de nivel superado
public class FinalRescate : MonoBehaviour
{
    [Header("Doña María y la novia")]
    // Doña María jalando a la novia de la mano (cuadro inicial); al soltarla queda con su cuadro de arrepentida
    [SerializeField] private SpriteRenderer madre;
    [SerializeField] private Sprite madreArrepentida;
    // Novia suelta: aparece junto a Doña María y corre hacia el jugador
    [SerializeField] private SpriteRenderer novia;
    [SerializeField] private Sprite[] cuadrosNovia;
    [SerializeField] private float velocidadNovia = 3.2f;

    [Header("Abrazo")]
    [SerializeField] private SpriteRenderer abrazo;
    [SerializeField] private Sprite[] cuadrosAbrazo;
    [SerializeField] private float velocidadEstudiante = 2.8f;
    [SerializeField] private float segundosDeAbrazo = 3.5f;

    void OnEnable()
    {
        BossController.JefeDerrotado += Iniciar;
    }

    void OnDisable()
    {
        BossController.JefeDerrotado -= Iniciar;
    }

    private void Iniciar()
    {
        StartCoroutine(Secuencia());
    }

    private IEnumerator Secuencia()
    {
        PlayerController jugador = FindFirstObjectByType<PlayerController>();
        if (jugador == null) yield break;

        Rigidbody2D rb = jugador.GetComponent<Rigidbody2D>();
        Animator animator = jugador.GetComponent<Animator>();
        SpriteRenderer srJugador = jugador.GetComponent<SpriteRenderer>();

        // Sin control del jugador: lo mueve la secuencia
        jugador.enabled = false;
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        if (animator != null) animator.SetFloat("Speed", 0f);
        yield return new WaitForSeconds(1.2f);

        // Doña María suelta la mano de la novia
        Vector3 inicioNovia = novia.transform.position;
        if (madre != null && madreArrepentida != null) madre.sprite = madreArrepentida;
        novia.gameObject.SetActive(true);
        novia.sprite = cuadrosNovia[0];
        novia.flipX = true;
        Sonido.Efecto("checkpoint");
        Chispas.Explotar(inicioNovia + Vector3.up * 0.6f, new Color(1f, 0.5f, 0.7f), 14, 3f);
        yield return new WaitForSeconds(1f);

        // Corren el uno hacia el otro hasta encontrarse a medio camino
        float puntoX = (rb.position.x + inicioNovia.x) * 0.5f;
        srJugador.flipX = false;
        float tiempo = 0f;
        while (Mathf.Abs(novia.transform.position.x - rb.position.x) > 0.9f)
        {
            tiempo += Time.deltaTime;
            novia.sprite = cuadrosNovia[(int)(tiempo * 10f) % cuadrosNovia.Length];
            novia.transform.position += Vector3.left * velocidadNovia * Time.deltaTime;
            rb.linearVelocity = new Vector2(velocidadEstudiante, rb.linearVelocity.y);
            if (animator != null) animator.SetFloat("Speed", 1f);
            yield return null;
        }
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        if (animator != null) animator.SetFloat("Speed", 0f);

        // Abrazo: se esconden los dos y aparece el cuadro del abrazo con corazones
        Vector3 posicionAbrazo = new Vector3((rb.position.x + novia.transform.position.x) * 0.5f, novia.transform.position.y, 0f);
        srJugador.enabled = false;
        novia.gameObject.SetActive(false);
        abrazo.transform.position = new Vector3(posicionAbrazo.x, abrazo.transform.position.y, 0f);
        abrazo.gameObject.SetActive(true);
        Sonido.Efecto("victoria");

        for (float t = 0f; t < segundosDeAbrazo; t += Time.deltaTime)
        {
            if (cuadrosAbrazo != null && cuadrosAbrazo.Length > 0) abrazo.sprite = cuadrosAbrazo[(int)(t * 2.5f) % cuadrosAbrazo.Length];
            if (Random.value < 0.06f) Chispas.Explotar(abrazo.transform.position + Vector3.up * 1.2f, new Color(1f, 0.4f, 0.6f), 4, 2f);
            yield return null;
        }

        Partida.NivelCompletado();
    }
}
