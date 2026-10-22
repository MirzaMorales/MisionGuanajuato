using System.Collections;
using UnityEngine;

// Túnel de entrada de una escena: al empezar, el Estudiante sale por la puerta del túnel
// (aparece pequeño y transparente, camina hacia afuera y recupera su tamaño) hasta el punto donde está colocado en la escena.
// Es la animación contraria a la de LevelGoal. Se pone en el objeto del túnel.
public class EntradaTunel : MonoBehaviour
{
    // Posición X (mundo) de la puerta del túnel, por donde sale el jugador
    [SerializeField] private float puertaX;
    // Espera para que termine el fundido de pantalla de TransicionEscena antes de empezar a salir
    [SerializeField] private float retrasoInicial = 0.7f;
    [SerializeField] private float velocidad = 2.5f;
    [SerializeField] private float duracionAparecer = 0.45f;
    [SerializeField, Range(0.3f, 1f)] private float escalaInicial = 0.75f;

    IEnumerator Start()
    {
        PlayerController jugador = FindFirstObjectByType<PlayerController>();
        if (jugador == null) yield break;

        Rigidbody2D rb = jugador.GetComponent<Rigidbody2D>();
        Animator animator = jugador.GetComponent<Animator>();
        SpriteRenderer sr = jugador.GetComponent<SpriteRenderer>();
        Transform tr = jugador.transform;

        // El punto donde está colocado el Estudiante en la escena es el destino (y su primer punto de reaparición)
        float destinoX = tr.position.x;
        Vector3 escalaFinal = tr.localScale;

        // Mientras sale no hay control, y empieza dentro del túnel: transparente y más pequeño
        jugador.enabled = false;
        rb.position = new Vector2(puertaX, rb.position.y);
        tr.position = new Vector3(puertaX, tr.position.y, tr.position.z);
        rb.linearVelocity = Vector2.zero;
        AplicarAparicion(sr, tr, escalaFinal, 0f);

        yield return new WaitForSeconds(retrasoInicial);

        if (animator != null) animator.SetFloat("Speed", 1f);
        sr.flipX = false;

        float t = 0f;
        while (rb.position.x < destinoX)
        {
            t += Time.deltaTime;
            AplicarAparicion(sr, tr, escalaFinal, Mathf.Clamp01(t / duracionAparecer));
            rb.linearVelocity = new Vector2(velocidad, rb.linearVelocity.y);
            yield return null;
        }

        rb.linearVelocity = Vector2.zero;
        AplicarAparicion(sr, tr, escalaFinal, 1f);
        if (animator != null) animator.SetFloat("Speed", 0f);
        jugador.enabled = true;
    }

    // k = 0: invisible y pequeño; k = 1: visible y de tamaño normal
    private void AplicarAparicion(SpriteRenderer sr, Transform tr, Vector3 escalaFinal, float k)
    {
        Color c = sr.color;
        c.a = k;
        sr.color = c;
        tr.localScale = Vector3.Lerp(escalaFinal * escalaInicial, escalaFinal, k);
    }
}
