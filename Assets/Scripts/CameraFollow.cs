using UnityEngine;

// Cámara de seguimiento suave que nunca se sale del escenario:
// - Izquierda y derecha: hasta donde llega el piso del nivel (los objetos "Suelo*"), así no se ve el fondo azul vacío.
// - Abajo: hasta donde terminan las piedras y ladrillos del piso. Si el jugador cae a un hueco, la cámara se queda ahí.
// - Arriba: hasta donde llega el cielo (FondoCielo).
// - Si la pantalla es más ancha que el nivel (celulares alargados), se acerca un poco para que no sobre azul a los lados.
// Los límites se calculan solos al iniciar la escena a partir de los sprites, sin tocar las escenas
public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform objetivo;
    [SerializeField] private float tiempoSuavizado = 0.15f;
    [SerializeField] private bool usarLimites = true;

    private Vector3 velocidadActual;
    private Camera camara;
    private float tamanoBase;
    private bool hayLimites;
    private float izquierda, derecha, abajo, arriba;

    // Otros scripts (ej. IntroPersecucion) cambian a quién sigue la cámara y luego lo devuelven
    public Transform Objetivo
    {
        get => objetivo;
        set => objetivo = value;
    }

    void Awake()
    {
        camara = GetComponent<Camera>();
        tamanoBase = camara != null ? camara.orthographicSize : 5f;
    }

    void Start()
    {
        if (usarLimites) CalcularLimites();
    }

    private void CalcularLimites()
    {
        bool hayPiso = false;
        Bounds piso = new Bounds();
        float techoCielo = float.MaxValue;

        foreach (SpriteRenderer sr in FindObjectsByType<SpriteRenderer>())
        {
            string nombre = sr.gameObject.name;
            if (nombre.StartsWith("Suelo"))
            {
                if (!hayPiso) { piso = sr.bounds; hayPiso = true; }
                else piso.Encapsulate(sr.bounds);
            }
            else if (nombre == "FondoCielo")
            {
                techoCielo = Mathf.Min(techoCielo, sr.bounds.max.y);
            }
        }

        // Sin piso (escena distinta a las de nivel) la cámara queda libre como antes
        if (!hayPiso) return;

        izquierda = piso.min.x;
        derecha = piso.max.x;
        abajo = piso.min.y;
        arriba = techoCielo;
        hayLimites = true;
    }

    void LateUpdate()
    {
        if (objetivo == null) return;

        AjustarZoom();

        // Solo seguimos en X e Y; la Z de la cámara se queda como está
        Vector3 destino = new Vector3(
            objetivo.position.x,
            objetivo.position.y,
            transform.position.z
        );
        if (hayLimites) destino = Limitar(destino);

        transform.position = Vector3.SmoothDamp(
            transform.position, destino, ref velocidadActual, tiempoSuavizado
        );
        if (hayLimites) transform.position = Limitar(transform.position);
    }

    // En pantallas muy anchas el nivel puede quedar más angosto que la vista: se acerca para llenarla
    private void AjustarZoom()
    {
        if (camara == null || !camara.orthographic) return;

        float tamano = tamanoBase;
        if (hayLimites)
        {
            float anchoNivel = derecha - izquierda;
            tamano = Mathf.Min(tamanoBase, anchoNivel / (2f * camara.aspect));
        }
        camara.orthographicSize = tamano;
    }

    // Mantiene el borde de la vista dentro del escenario
    private Vector3 Limitar(Vector3 p)
    {
        if (camara == null) return p;

        float mitadAlto = camara.orthographicSize;
        float mitadAncho = mitadAlto * camara.aspect;

        p.x = LimitarEje(p.x, izquierda + mitadAncho, derecha - mitadAncho);
        p.y = LimitarEje(p.y, abajo + mitadAlto, arriba - mitadAlto);
        return p;
    }

    // Si el escenario es más chico que la vista, la cámara se queda en el centro
    private static float LimitarEje(float valor, float minimo, float maximo)
    {
        if (minimo > maximo) return (minimo + maximo) * 0.5f;
        return Mathf.Clamp(valor, minimo, maximo);
    }
}
