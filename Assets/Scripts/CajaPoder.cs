using System.Collections;
using UnityEngine;

// Caja con una mochila dibujada: al golpearla desde abajo se vacía y suelta una mochila que da el poder de lanzar comida
// (F en el teclado, el botón de la fresa en el celular). Una caja solo se puede golpear una vez.
// Partida se la agrega sola a todo objeto de la escena cuyo nombre empiece con "CajaPoder"
[RequireComponent(typeof(SpriteRenderer))]
public class CajaPoder : MonoBehaviour
{
    [SerializeField] private Color colorUsada = new Color(0.55f, 0.55f, 0.55f, 1f);
    [SerializeField] private float alturaRebote = 0.15f;
    [SerializeField] private float alturaSalida = 1.0f;
    [SerializeField] private float duracionSalida = 0.35f;
    [SerializeField] private float escalaMochila = 0.4f;

    private SpriteRenderer sr;
    private bool usada;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (usada) return;
        if (GolpeDeCaja.EsGolpeDesdeAbajo(col, transform)) Golpear();
    }

    private void Golpear()
    {
        usada = true;
        Sonido.Efecto("caja");
        sr.color = colorUsada;
        StartCoroutine(Rebotar());
        StartCoroutine(SacarMochila());
    }

    private IEnumerator Rebotar()
    {
        Vector3 inicio = transform.position;
        float duracion = 0.15f;
        for (float t = 0f; t < duracion; t += Time.deltaTime)
        {
            transform.position = inicio + Vector3.up * (alturaRebote * Mathf.Sin(t / duracion * Mathf.PI));
            yield return null;
        }
        transform.position = inicio;
    }

    // La mochila sale de la caja hacia arriba, gira y queda lista para recogerse
    private IEnumerator SacarMochila()
    {
        DatosPoderComida datos = DatosPoderComida.Actual;
        if (datos == null || datos.mochila == null || datos.mochila.Length == 0) yield break;

        Vector3 inicio = transform.position;
        GameObject go = new GameObject("Mochila");
        go.transform.position = inicio;
        go.transform.localScale = Vector3.one * escalaMochila;

        SpriteRenderer spr = go.AddComponent<SpriteRenderer>();
        spr.sprite = datos.mochila[0];
        spr.sortingOrder = sr.sortingOrder + 1;

        BoxCollider2D c = go.AddComponent<BoxCollider2D>();
        c.isTrigger = true;
        c.size = new Vector2(1.0f, 1.4f);

        go.AddComponent<ItemMochila>().Iniciar(datos.mochila);

        for (float t = 0f; t < duracionSalida && go != null; t += Time.deltaTime)
        {
            go.transform.position = inicio + Vector3.up * (alturaSalida * Mathf.Clamp01(t / duracionSalida));
            yield return null;
        }
    }
}

// La mochila que sale de la caja: gira con sus cuadros y, al tocarla el Estudiante, le da el poder de lanzar comida
public class ItemMochila : MonoBehaviour
{
    private Sprite[] cuadros;
    private SpriteRenderer sr;
    private bool recogida;

    public void Iniciar(Sprite[] cuadros)
    {
        this.cuadros = cuadros;
        sr = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (cuadros == null || cuadros.Length == 0) return;
        sr.sprite = cuadros[Mathf.FloorToInt(Time.time * 10f) % cuadros.Length];
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (recogida || col.GetComponent<PlayerController>() == null) return;

        recogida = true;
        PoderComida.Activar();
        Chispas.Explotar(transform.position, new Color(1f, 0.5f, 0.3f), 14);
        HUDJuego.Aviso(ControlesMoviles.Activos
            ? "¡Poder de lanzar comida! Usa el botón de la fresa"
            : "¡Poder de lanzar comida! Presiona F");
        Destroy(gameObject);
    }
}
