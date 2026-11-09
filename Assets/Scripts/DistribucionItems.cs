using System.Collections.Generic;
using UnityEngine;

// Reparte los premios de cada nivel entre sus escenas, sin editar las escenas:
// - Cajas de mermelada: en 2 de las escenas de cada nivel.
// - Cajas de mochila (poder de lanzar comida): en 2 de las escenas de cada nivel.
// - Corazón de vida: uno solo, en 2 de las escenas de cada nivel.
// Qué escenas toca cada premio se sortea al empezar la partida y se conserva hasta la siguiente partida nueva
// (así, si el jugador reintenta una escena, todo queda donde estaba).
// La escena del jefe no se toca. Lo llama Partida al cargar cada escena de nivel
public static class DistribucionItems
{
    private const int EscenasPorPremio = 2;

    private class Reparto
    {
        public HashSet<string> mermelada;
        public HashSet<string> mochila;
        public HashSet<string> vida;
    }

    private static readonly Dictionary<int, Reparto> repartos = new Dictionary<int, Reparto>();

    public static void Reiniciar() => repartos.Clear();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ReiniciarAlIniciar() => repartos.Clear();

    public static void Aplicar(Partida.InfoEscena info)
    {
        List<string> escenas = Partida.EscenasDelNivel(info.numeroNivel);
        // La escena del jefe no está en la lista: no se reparte nada ahí
        if (!escenas.Contains(info.escena)) return;

        if (!repartos.TryGetValue(info.numeroNivel, out Reparto r))
        {
            r = new Reparto { mermelada = Sortear(escenas), mochila = Sortear(escenas), vida = Sortear(escenas) };
            repartos[info.numeroNivel] = r;
        }

        AplicarMermeladaYMochila(info.escena, r);
        AplicarVida(info.escena, r);
    }

    // Elige al azar las escenas que llevan el premio
    private static HashSet<string> Sortear(List<string> escenas)
    {
        List<string> baraja = new List<string>(escenas);
        for (int i = baraja.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (baraja[i], baraja[j]) = (baraja[j], baraja[i]);
        }
        return new HashSet<string>(baraja.GetRange(0, Mathf.Min(EscenasPorPremio, baraja.Count)));
    }

    // ---------- Mermelada y mochila (cajas) ----------

    private static void AplicarMermeladaYMochila(string escena, Reparto r)
    {
        bool quiereMermelada = r.mermelada.Contains(escena);
        bool quiereMochila = r.mochila.Contains(escena);

        CajaMermelada mermelada = Object.FindFirstObjectByType<CajaMermelada>();

        // Cajas de mochila ya puestas en la escena (como la del Nivel 2): se quedan o se quitan según el sorteo
        bool hayMochila = false;
        foreach (SpriteRenderer sr in Object.FindObjectsByType<SpriteRenderer>())
        {
            if (!sr.gameObject.name.StartsWith("CajaPoder")) continue;
            if (quiereMochila && !hayMochila)
            {
                hayMochila = true;
                if (sr.GetComponent<CajaPoder>() == null) sr.gameObject.AddComponent<CajaPoder>();
            }
            else
            {
                sr.gameObject.SetActive(false);
            }
        }

        // Si toca mochila y no había caja, se hace una copia de la caja de mermelada con la imagen de la mochila
        if (quiereMochila && !hayMochila && mermelada != null) CrearCajaMochila(mermelada, quiereMermelada);

        if (!quiereMermelada && mermelada != null) mermelada.Desactivar();
    }

    private static void CrearCajaMochila(CajaMermelada plantilla, bool laMermeladaSeQueda)
    {
        DatosPoderComida datos = DatosPoderComida.Actual;
        if (datos == null || datos.cajaPoder == null) return;

        // Si la caja de mermelada se queda, la mochila va a un lado en un hueco libre; si no, ocupa su lugar
        Vector3 posicion = plantilla.transform.position;
        if (laMermeladaSeQueda && !BuscarHuecoLibre(posicion, out posicion)) return;

        // Se copia con la plantilla apagada para que la copia no ejecute el script de la caja de mermelada
        GameObject original = plantilla.gameObject;
        bool estabaActiva = original.activeSelf;
        original.SetActive(false);
        GameObject copia = Object.Instantiate(original, posicion, original.transform.rotation);
        original.SetActive(estabaActiva);

        Object.DestroyImmediate(copia.GetComponent<CajaMermelada>());
        copia.name = "CajaPoder_Aleatoria";
        SpriteRenderer sr = copia.GetComponent<SpriteRenderer>();
        sr.sprite = datos.cajaPoder;
        sr.color = Color.white;
        copia.AddComponent<CajaPoder>();
        copia.SetActive(true);
    }

    // Busca a los lados de la caja un lugar con piso debajo y sin nada que estorbe (otras cajas, obstáculos, paredes)
    private static bool BuscarHuecoLibre(Vector3 centro, out Vector3 resultado)
    {
        float[] distancias = { 3.5f, -3.5f, 5f, -5f, 6.5f, -6.5f, 8f, -8f, 10f, -10f };
        int capaSuelo = LayerMask.GetMask("Suelo");

        foreach (float dx in distancias)
        {
            Vector3 p = centro + Vector3.right * dx;
            bool hayPiso = Physics2D.Raycast(p, Vector2.down, 12f, capaSuelo).collider != null;
            bool libre = Physics2D.OverlapBox(p, new Vector2(2.4f, 1.4f), 0f) == null;
            if (hayPiso && libre)
            {
                resultado = p;
                return true;
            }
        }
        resultado = centro;
        return false;
    }

    // ---------- Corazón de vida ----------

    private static void AplicarVida(string escena, Reparto r)
    {
        bool quiere = r.vida.Contains(escena);
        ItemVida[] corazones = Object.FindObjectsByType<ItemVida>();

        if (!quiere)
        {
            foreach (ItemVida c in corazones) c.gameObject.SetActive(false);
            return;
        }
        if (corazones.Length > 0) return;

        CrearCorazon();
    }

    // Escenas que no traían corazón (Nivel 1): se coloca uno en un punto libre del piso, lejos del inicio
    private static void CrearCorazon()
    {
        DatosPoderComida datos = DatosPoderComida.Actual;
        PlayerController jugador = Object.FindFirstObjectByType<PlayerController>();
        if (datos == null || datos.corazon == null || jugador == null) return;

        // Hasta dónde llega el piso a la derecha
        float derecha = jugador.transform.position.x + 30f;
        foreach (SpriteRenderer sr in Object.FindObjectsByType<SpriteRenderer>())
        {
            if (sr.gameObject.name.StartsWith("Suelo")) derecha = Mathf.Max(derecha, sr.bounds.max.x);
        }

        float desde = jugador.transform.position.x + 10f;
        float hasta = derecha - 8f;
        if (hasta <= desde) return;

        int capaSuelo = LayerMask.GetMask("Suelo");
        for (int intento = 0; intento < 40; intento++)
        {
            float x = Random.Range(desde, hasta);
            RaycastHit2D piso = Physics2D.Raycast(new Vector2(x, 6f), Vector2.down, 14f, capaSuelo);
            if (piso.collider == null) continue;

            Vector2 p = new Vector2(x, piso.point.y + 1.1f);
            // Que no quede encima de un obstáculo, una fresa u otro premio
            if (Physics2D.OverlapCircle(p, 0.8f) != null) continue;

            GameObject go = new GameObject("Vida_Aleatoria");
            go.transform.position = p;
            go.transform.localScale = Vector3.one * 0.5f;

            SpriteRenderer spr = go.AddComponent<SpriteRenderer>();
            spr.sprite = datos.corazon;
            spr.sortingOrder = 1;

            CircleCollider2D c = go.AddComponent<CircleCollider2D>();
            c.isTrigger = true;
            c.radius = 0.8f;

            go.AddComponent<ItemVida>();
            go.AddComponent<FlotarItem>();
            return;
        }
    }
}

// Sube y baja suavemente, como los corazones del resto de los niveles
public class FlotarItem : MonoBehaviour
{
    private Vector3 inicio;

    void Start() => inicio = transform.position;

    void Update()
    {
        transform.position = inicio + Vector3.up * (Mathf.Sin(Time.time * 3f) * 0.12f);
    }
}
