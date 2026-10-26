using UnityEngine;

// Cameo de Doña María llevándose a la novia (HU27): cuando el jugador llega cerca del final del nivel aparece a lo lejos,
// corre jalando a la novia hacia la derecha y se pierde de vista. Es un adorno de la historia: no hace daño ni bloquea nada
public class CameoHuida : MonoBehaviour
{
    // El cameo empieza cuando el jugador pasa de esta posición X
    [SerializeField] private float xDisparo = 40f;
    // Cuántas unidades por delante del jugador aparece
    [SerializeField] private float distanciaInicial = 9f;
    [SerializeField] private float velocidad = 5.5f;
    [SerializeField] private float distanciaTotal = 16f;
    [SerializeField] private float alturaPaso = 0.12f;
    [SerializeField] private string aviso = "¡Doña María se lleva a tu novia!";

    private Transform jugador;
    private SpriteRenderer sr;
    private bool corriendo;
    private bool termino;
    private float xInicial;
    private float yBase;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        sr.enabled = false;
    }

    void Start()
    {
        PlayerController pc = FindFirstObjectByType<PlayerController>();
        if (pc != null) jugador = pc.transform;
        yBase = transform.position.y;
    }

    void Update()
    {
        if (termino || jugador == null) return;

        if (!corriendo)
        {
            if (jugador.position.x < xDisparo) return;
            corriendo = true;
            xInicial = jugador.position.x + distanciaInicial;
            transform.position = new Vector3(xInicial, yBase, transform.position.z);
            // El cuadro mira a la izquierda: se voltea para correr hacia la derecha
            sr.flipX = true;
            sr.enabled = true;
            Sonido.Efecto("rata", 0.6f);
            HUDJuego.Aviso(aviso);
        }

        float avance = transform.position.x - xInicial + velocidad * Time.deltaTime;
        transform.position = new Vector3(xInicial + avance, yBase + Mathf.Abs(Mathf.Sin(Time.time * 14f)) * alturaPaso, transform.position.z);
        if (avance >= distanciaTotal)
        {
            termino = true;
            sr.enabled = false;
        }
    }
}
