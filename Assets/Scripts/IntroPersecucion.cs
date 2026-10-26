using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// Inicio de nivel con historia (HU27): después del letrero que presenta el lugar, y antes de que el jugador pueda moverse,
// la cámara sigue un momento a Doña María llevándose a la novia de la mano hasta una puerta, ellas entran y la cámara
// regresa al Estudiante, que ya puede jugar. Solo se ve una vez por partida en cada escena (al reintentar no se repite);
// se puede saltar con Espacio, Enter o clic
public class IntroPersecucion : MonoBehaviour
{
    [SerializeField] private SpriteRenderer pareja;
    [SerializeField] private Transform puerta;
    [SerializeField] private PuertaAnimada puertaAnimada;
    [SerializeField] private float velocidad = 3f;
    [SerializeField] private float alturaPaso = 0.1f;

    private static readonly HashSet<string> escenasVistas = new HashSet<string>();

    private bool saltar;

    // Partida nueva desde el menú: la intro vuelve a verse
    public static void Reiniciar()
    {
        escenasVistas.Clear();
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ReiniciarAlIniciar()
    {
        escenasVistas.Clear();
    }

    IEnumerator Start()
    {
        string escena = SceneManager.GetActiveScene().name;
        PlayerController jugador = FindFirstObjectByType<PlayerController>();
        CameraFollow camara = FindFirstObjectByType<CameraFollow>();
        if (jugador == null || camara == null || escenasVistas.Contains(escena))
        {
            if (pareja != null) pareja.gameObject.SetActive(false);
            yield break;
        }
        escenasVistas.Add(escena);

        Rigidbody2D rb = jugador.GetComponent<Rigidbody2D>();
        Animator animator = jugador.GetComponent<Animator>();

        // Sin control desde el principio: mientras sale el letrero y, si la escena lo tiene, el Estudiante sale del túnel
        yield return null;
        float inicio = Time.time;
        while (HUDJuego.LetreroActivo || Time.time - inicio < 1f || EntradaDelTunelEnCurso())
        {
            jugador.enabled = false;
            yield return null;
        }

        jugador.enabled = false;
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        if (animator != null) animator.SetFloat("Speed", 0f);
        yield return Esperar(0.25f);

        // La cámara deja al jugador y sigue a Doña María
        Transform objetivoOriginal = camara.Objetivo;
        camara.Objetivo = pareja.transform;
        yield return Esperar(0.9f);

        // Camina jalando a la novia hasta la puerta
        pareja.flipX = true;
        Transform tp = pareja.transform;
        float yBase = tp.position.y;
        float destino = puerta.position.x;
        while (tp.position.x < destino - 0.1f && !saltar)
        {
            float x = Mathf.MoveTowards(tp.position.x, destino, velocidad * Time.deltaTime);
            tp.position = new Vector3(x, yBase + Mathf.Abs(Mathf.Sin(Time.time * 9f)) * alturaPaso, tp.position.z);
            RevisarSalto();
            yield return null;
        }

        // Se abre la puerta y entran: se hacen pequeñas y transparentes
        if (!saltar)
        {
            if (puertaAnimada != null) puertaAnimada.Abrir();
            yield return Esperar(0.4f);
            Vector3 escalaInicial = tp.localScale;
            for (float t = 0f; t < 0.7f && !saltar; t += Time.deltaTime)
            {
                float k = t / 0.7f;
                Color c = pareja.color;
                c.a = 1f - k;
                pareja.color = c;
                tp.localScale = Vector3.Lerp(escalaInicial, escalaInicial * 0.8f, k);
                RevisarSalto();
                yield return null;
            }
            yield return Esperar(0.3f);
        }
        pareja.gameObject.SetActive(false);
        if (puertaAnimada != null) puertaAnimada.Cerrar();

        // La cámara regresa al Estudiante y, cuando llega, ya puede moverse
        camara.Objetivo = objetivoOriginal;
        for (float t = 0f; t < 2.5f && Mathf.Abs(camara.transform.position.x - jugador.transform.position.x) > 0.4f && !saltar; t += Time.deltaTime)
        {
            RevisarSalto();
            yield return null;
        }
        jugador.enabled = true;
    }

    private bool EntradaDelTunelEnCurso()
    {
        foreach (EntradaTunel t in FindObjectsByType<EntradaTunel>(FindObjectsSortMode.None))
        {
            if (t.EnCurso) return true;
        }
        return false;
    }

    // Espera segundos sin dejar de revisar si se pide saltar la intro
    private IEnumerator Esperar(float segundos)
    {
        for (float t = 0f; t < segundos && !saltar; t += Time.deltaTime)
        {
            RevisarSalto();
            yield return null;
        }
    }

    private void RevisarSalto()
    {
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return) || Input.GetMouseButtonDown(0)) saltar = true;
    }
}
