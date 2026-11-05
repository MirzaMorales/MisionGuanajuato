using UnityEngine;

// Llave que se recoge para activar la meta del nivel (HU15). Requiere un Collider2D con "Is Trigger" activado.
// Gira con sus cuadros y flota un poco; al tocarla el jugador queda con la llave (Llave.Tiene) y el HUD la muestra
public class KeyItem : MonoBehaviour
{
    [SerializeField] private int puntos = 50;
    [SerializeField] private Sprite[] cuadrosGiro;
    [SerializeField] private float cuadrosPorSegundo = 8f;
    [SerializeField] private float amplitudFlotar = 0.12f;

    private SpriteRenderer sr;
    private Vector3 posicionInicial;
    private float relojCuadro;
    private int cuadro;
    private bool recogida;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        posicionInicial = transform.position;
    }

    void Update()
    {
        if (recogida) return;

        transform.position = posicionInicial + Vector3.up * Mathf.Sin(Time.time * 3f) * amplitudFlotar;

        if (sr == null || cuadrosGiro == null || cuadrosGiro.Length == 0) return;
        relojCuadro += Time.deltaTime;
        if (relojCuadro >= 1f / cuadrosPorSegundo)
        {
            relojCuadro = 0f;
            cuadro = (cuadro + 1) % cuadrosGiro.Length;
            sr.sprite = cuadrosGiro[cuadro];
        }
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (recogida) return;
        // Solo el jugador la recoge (el Estudiante no tiene tag, se reconoce por su controlador)
        if (col.GetComponent<PlayerController>() == null) return;

        recogida = true;
        Llave.Obtener();
        Puntuacion.Sumar(puntos);
        Sonido.Efecto("llave");
        Chispas.Explotar(transform.position, new Color(1f, 0.85f, 0.2f), 14, 3f);
        HUDJuego.Aviso("¡Tienes la llave de la Parroquia!");

        Destroy(gameObject);
    }
}
