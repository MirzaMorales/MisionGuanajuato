using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

// Requiere un Collider2D con "Is Trigger" activado. Al tocarlo el jugador se guarda el nivel como completado,
// "entra" por la puerta o el túnel (camina hasta el centro y se desvanece) y se carga la siguiente escena con un fundido
public class LevelGoal : MonoBehaviour
{
    // Si se deja vacío se carga la siguiente escena según el orden de File > Build Settings
    [SerializeField] private string escenaSiguiente;

    [Header("Nivel dividido en varias escenas")]
    // Solo la última escena de cada nivel lo marca como completado
    [SerializeField] private bool esUltimaEscenaDelNivel = true;
    // Nombre con el que se guarda el nivel (ej. "Nivel1"); si se deja vacío se usa el nombre de la escena
    [SerializeField] private string nombreNivel;

    [Header("Animación de entrada")]
    // Puerta opcional que se abre al llegar a la meta (ej. la puerta de la Central de Autobuses)
    [SerializeField] private PuertaAnimada puerta;
    [SerializeField] private float velocidadEntrada = 2.5f;
    [SerializeField] private float duracionDesvanecer = 0.45f;
    // Tamaño final del jugador al desvanecerse, para dar la sensación de que se adentra en el túnel
    [SerializeField, Range(0.3f, 1f)] private float escalaFinal = 0.75f;
    // Segundos que se espera antes de empezar el fundido de pantalla
    [SerializeField] private float esperaAntesDelFundido = 0.3f;
    // Cuadros del Estudiante de espaldas: al entrar se da la vuelta y camina hacia adentro con estos
    [SerializeField] private Sprite[] spritesEspalda;
    [SerializeField] private float cuadrosPorSegundo = 8f;

    private const string PrefijoClave = "NivelCompletado_";

    private bool activada;

    // Otros sistemas (selector de niveles, pantalla de victoria) pueden consultar si un nivel ya se terminó
    public static bool NivelCompletado(string nombreEscena)
    {
        return PlayerPrefs.GetInt(PrefijoClave + nombreEscena, 0) == 1;
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (activada) return;
        // El Estudiante no tiene tag, se reconoce por su controlador
        PlayerController jugador = col.GetComponent<PlayerController>();
        if (jugador == null) return;

        activada = true;
        if (esUltimaEscenaDelNivel) GuardarNivelCompletado();
        StartCoroutine(Secuencia(jugador));
    }

    private void GuardarNivelCompletado()
    {
        string nombreActual = string.IsNullOrEmpty(nombreNivel) ? SceneManager.GetActiveScene().name : nombreNivel;
        PlayerPrefs.SetInt(PrefijoClave + nombreActual, 1);
        PlayerPrefs.Save();
        Debug.Log($"Nivel completado y guardado: {nombreActual}");
    }

    private IEnumerator Secuencia(PlayerController jugador)
    {
        Rigidbody2D rb = jugador.GetComponent<Rigidbody2D>();
        Animator animator = jugador.GetComponent<Animator>();
        SpriteRenderer sr = jugador.GetComponent<SpriteRenderer>();

        // Sin control del jugador: la animación lo mueve
        jugador.enabled = false;
        if (puerta != null) puerta.Abrir();

        yield return CaminarAlCentro(rb, animator, sr);
        yield return Desvanecer(rb, animator, sr, jugador.transform);
        yield return new WaitForSeconds(esperaAntesDelFundido);

        if (!string.IsNullOrEmpty(escenaSiguiente))
        {
            TransicionEscena.Cargar(escenaSiguiente);
            yield break;
        }

        int siguiente = SceneManager.GetActiveScene().buildIndex + 1;
        if (siguiente < SceneManager.sceneCountInBuildSettings)
        {
            TransicionEscena.Cargar(siguiente);
        }
        else
        {
            // Todavía no existe el siguiente nivel: se reinicia esta escena para poder seguir probando
            Debug.LogWarning("LevelGoal: no hay una escena siguiente en Build Settings, se reinicia la escena actual", this);
            TransicionEscena.Cargar(SceneManager.GetActiveScene().buildIndex);
        }
    }

    // El jugador camina hasta el centro de la meta (la puerta del túnel o del edificio)
    private IEnumerator CaminarAlCentro(Rigidbody2D rb, Animator animator, SpriteRenderer sr)
    {
        float objetivoX = transform.position.x;
        if (animator != null) animator.SetFloat("Speed", 1f);

        while (Mathf.Abs(rb.position.x - objetivoX) > 0.05f)
        {
            float direccion = Mathf.Sign(objetivoX - rb.position.x);
            if (sr != null) sr.flipX = direccion < 0f;
            rb.linearVelocity = new Vector2(direccion * velocidadEntrada, rb.linearVelocity.y);
            yield return new WaitForFixedUpdate();
        }

        rb.linearVelocity = Vector2.zero;
    }

    // Camina en el sitio mientras se hace transparente y más pequeño: como si se adentrara
    private IEnumerator Desvanecer(Rigidbody2D rb, Animator animator, SpriteRenderer sr, Transform jugador)
    {
        if (sr == null) yield break;

        // PlayerHealth hace parpadear el sprite tras un golpe: se asegura de que esté visible
        sr.enabled = true;

        // Se da la vuelta: el Animator se apaga para que no pise el sprite y se usan los cuadros de espaldas
        bool deEspaldas = spritesEspalda != null && spritesEspalda.Length > 0;
        if (deEspaldas)
        {
            if (animator != null) animator.enabled = false;
            sr.flipX = false;
        }

        Vector3 escalaInicial = jugador.localScale;
        float t = 0f;
        while (t < duracionDesvanecer)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / duracionDesvanecer);
            if (deEspaldas) sr.sprite = spritesEspalda[(int)(t * cuadrosPorSegundo) % spritesEspalda.Length];
            Color c = sr.color;
            c.a = 1f - k;
            sr.color = c;
            jugador.localScale = Vector3.Lerp(escalaInicial, escalaInicial * escalaFinal, k);
            rb.linearVelocity = Vector2.zero;
            yield return null;
        }

        Color final = sr.color;
        final.a = 0f;
        sr.color = final;
        if (animator != null && animator.enabled) animator.SetFloat("Speed", 0f);
    }
}
