using System.Collections;
using UnityEngine;

// Caja café tipo bloque de monedas de Mario: cada golpe desde abajo suelta una fresa que suma puntos.
// Se puede golpear varias veces hasta que se acaben las fresas; entonces se oscurece
[RequireComponent(typeof(SpriteRenderer))]
public class CajaFresas : MonoBehaviour
{
    // Fresa de adorno (sprite + animación) que está en la escena desactivada; se copia en cada golpe
    [SerializeField] private GameObject fresaPremio;
    [SerializeField] private int golpesMaximos = 3;
    [SerializeField] private int puntosPorFresa = 10;
    [SerializeField] private Color colorVacia = new Color(0.55f, 0.55f, 0.55f, 1f);

    [Header("Animación")]
    [SerializeField] private float alturaRebote = 0.15f;
    [SerializeField] private float alturaSalida = 1.1f;
    [SerializeField] private float duracionSalida = 0.45f;

    private SpriteRenderer sr;
    private int golpes;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        if (fresaPremio != null) fresaPremio.SetActive(false);
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (golpes >= golpesMaximos) return;
        if (GolpeDeCaja.EsGolpeDesdeAbajo(col, transform)) Golpear();
    }

    private void Golpear()
    {
        golpes++;
        if (golpes >= golpesMaximos) sr.color = colorVacia;

        StartCoroutine(Rebotar());
        if (fresaPremio != null) StartCoroutine(SoltarFresa());
    }

    private IEnumerator Rebotar()
    {
        Vector3 inicio = transform.position;
        float t = 0f;
        float duracion = 0.15f;
        while (t < duracion)
        {
            t += Time.deltaTime;
            float k = Mathf.Sin(Mathf.Clamp01(t / duracion) * Mathf.PI);
            transform.position = inicio + Vector3.up * (alturaRebote * k);
            yield return null;
        }
        transform.position = inicio;
    }

    // La fresa sube desde detrás de la caja, suma sus puntos al llegar arriba y desaparece
    private IEnumerator SoltarFresa()
    {
        Vector3 inicio = transform.position;
        GameObject fresa = Instantiate(fresaPremio, inicio, Quaternion.identity);
        fresa.SetActive(true);

        float t = 0f;
        while (t < duracionSalida)
        {
            t += Time.deltaTime;
            fresa.transform.position = inicio + Vector3.up * (alturaSalida * Mathf.Clamp01(t / duracionSalida));
            yield return null;
        }

        Puntuacion.Sumar(puntosPorFresa);
        Destroy(fresa);
    }
}
