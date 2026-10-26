using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// Inicio de la última escena de cada nivel (HU27): después del letrero que presenta el lugar, y antes de que el jugador
// pueda moverse, la cámara viaja hasta la puerta del final (la Central de Autobuses, el Pípila, la Nevería, la Parroquia),
// ahí Doña María lleva a la novia de la mano y cruzan la puerta, y la cámara regresa al Estudiante, que ya puede jugar.
// Solo se ve una vez por partida en cada escena (al reintentar no se repite); se puede saltar con Espacio, Enter o clic
public class IntroPersecucion : MonoBehaviour
{
    [Header("Doña María y la novia")]
    // Empiezan donde esté colocado este objeto y caminan hacia la derecha hasta la puerta
    [SerializeField] private SpriteRenderer pareja;
    [SerializeField] private float velocidad = 3f;
    [SerializeField] private float alturaPaso = 0.1f;

    [Header("Puerta del final")]
    [SerializeField] private Transform puertaPunto;
    // Opcional: si la puerta se anima (abre y cierra), se usa
    [SerializeField] private PuertaAnimada puertaAnimada;
    // Si la puerta está más arriba que el piso (escalinatas): desde esta X suben hasta la altura de la puerta
    [SerializeField] private bool suben;
    [SerializeField] private float xInicioSubida;
    [SerializeField] private float yPuerta;

    [Header("Cámara")]
    [SerializeField] private float velocidadCamara = 18f;
    // Altura que enfoca la cámara mientras ve a la pareja
    [SerializeField] private float camaraY = -0.99f;

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
        Transform tp = pareja.transform;
        Vector3 posicionInicial = tp.position;
        Vector3 escalaInicial = tp.localScale;

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

        // La cámara viaja desde el jugador hasta la puerta, siguiendo un punto que se mueve a velocidad constante
        Transform objetivoOriginal = camara.Objetivo;
        Transform guia = new GameObject("CamaraIntro").transform;
        guia.position = new Vector3(jugador.transform.position.x, jugador.transform.position.y, 0f);
        camara.Objetivo = guia;
        Vector3 llegada = new Vector3(posicionInicial.x + 3f, camaraY, 0f);
        while (Vector3.Distance(guia.position, llegada) > 0.05f && !saltar)
        {
            guia.position = Vector3.MoveTowards(guia.position, llegada, velocidadCamara * Time.deltaTime);
            RevisarSalto();
            yield return null;
        }
        yield return Esperar(0.7f);

        // Doña María camina jalando a la novia hasta la puerta
        pareja.flipX = true;
        float destino = puertaPunto.position.x;
        float yBase = posicionInicial.y;
        while (tp.position.x < destino - 0.1f && !saltar)
        {
            float x = Mathf.MoveTowards(tp.position.x, destino, velocidad * Time.deltaTime);
            float subida = suben && x > xInicioSubida ? Mathf.Lerp(yBase, yPuerta, (x - xInicioSubida) / Mathf.Max(0.01f, destino - xInicioSubida)) : yBase;
            tp.position = new Vector3(x, subida + Mathf.Abs(Mathf.Sin(Time.time * 9f)) * alturaPaso, tp.position.z);
            guia.position = new Vector3(x + 1.5f, camaraY, 0f);
            RevisarSalto();
            yield return null;
        }

        // Se abre la puerta (si la tiene) y entran: se hacen pequeñas y transparentes
        if (!saltar)
        {
            if (puertaAnimada != null) puertaAnimada.Abrir();
            yield return Esperar(0.5f);
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
            yield return Esperar(0.4f);
        }
        pareja.gameObject.SetActive(false);
        if (puertaAnimada != null) puertaAnimada.Cerrar();

        // La cámara regresa al Estudiante y, cuando llega, ya puede moverse
        while (Vector3.Distance(guia.position, jugador.transform.position) > 0.3f && !saltar)
        {
            Vector3 meta = new Vector3(jugador.transform.position.x, jugador.transform.position.y, 0f);
            guia.position = Vector3.MoveTowards(guia.position, meta, velocidadCamara * Time.deltaTime);
            RevisarSalto();
            yield return null;
        }
        camara.Objetivo = objetivoOriginal;
        Destroy(guia.gameObject);
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
