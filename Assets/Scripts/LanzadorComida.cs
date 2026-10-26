using UnityEngine;

// Poder de lanzar comida (HU29), va en el Estudiante de la escena del jefe. Con la tecla F lanza hacia donde mira una
// fresa, una charamusca o una nieve (en ese orden). La munición es infinita: no depende de lo que se recolectó
public class LanzadorComida : MonoBehaviour
{
    [SerializeField] private KeyCode tecla = KeyCode.F;
    [SerializeField] private Sprite[] comidas;
    [SerializeField] private float segundosEntreLanzamientos = 0.4f;
    [SerializeField] private float velocidadX = 9f;
    [SerializeField] private float velocidadY = 4.5f;
    [SerializeField] private Vector2 origen = new Vector2(0.45f, 0.35f);

    private SpriteRenderer sr;
    private float siguienteLanzamiento;
    private int indice;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (Time.timeScale <= 0f || !Input.GetKeyDown(tecla) || Time.time < siguienteLanzamiento) return;
        if (comidas == null || comidas.Length == 0) return;

        siguienteLanzamiento = Time.time + segundosEntreLanzamientos;
        float direccion = sr != null && sr.flipX ? -1f : 1f;

        GameObject go = new GameObject("Comida");
        go.transform.position = transform.position + new Vector3(direccion * origen.x, origen.y, 0f);
        go.transform.localScale = Vector3.one * 1.6f;

        SpriteRenderer spr = go.AddComponent<SpriteRenderer>();
        spr.sprite = comidas[indice];
        spr.sortingOrder = 5;
        indice = (indice + 1) % comidas.Length;

        CircleCollider2D c = go.AddComponent<CircleCollider2D>();
        c.isTrigger = true;
        c.radius = 0.2f;

        Rigidbody2D rb = go.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;

        go.AddComponent<ComidaProyectil>().Iniciar(new Vector2(direccion * velocidadX, velocidadY));
        Sonido.Efecto("lanzar", 0.8f);
    }
}
