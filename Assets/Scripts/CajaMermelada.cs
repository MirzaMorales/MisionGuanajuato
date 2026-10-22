using System.Collections;
using UnityEngine;

// Caja dorada tipo bloque de Mario: al golpearla desde abajo con la cabeza se vacía y suelta el envase de mermelada
// (el Estudiante crece). Una caja solo se puede golpear una vez.
// El objeto del premio está en la escena, desactivado, en la posición de la caja
[RequireComponent(typeof(SpriteRenderer))]
public class CajaMermelada : MonoBehaviour
{
    [SerializeField] private Sprite spriteUsada;
    [SerializeField] private Color colorUsada = new Color(0.55f, 0.55f, 0.55f, 1f);
    [SerializeField] private GameObject itemCrecimiento;
    // Opcional: un corazón de vida extra que puede salir en lugar del envase (0 = siempre el envase)
    [SerializeField] private GameObject itemVida;
    [SerializeField, Range(0f, 1f)] private float probabilidadVida = 0f;

    [Header("Animación")]
    [SerializeField] private float alturaRebote = 0.15f;
    [SerializeField] private float alturaSalidaItem = 1.0f;
    [SerializeField] private float duracionSalida = 0.35f;

    private SpriteRenderer sr;
    private bool usada;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        if (itemVida != null) itemVida.SetActive(false);
        if (itemCrecimiento != null) itemCrecimiento.SetActive(false);
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (usada) return;
        if (GolpeDeCaja.EsGolpeDesdeAbajo(col, transform)) Golpear();
    }

    private void Golpear()
    {
        usada = true;
        if (spriteUsada != null) sr.sprite = spriteUsada;
        sr.color = colorUsada;

        bool sale_corazon = itemVida != null && Random.value < probabilidadVida;
        GameObject item = sale_corazon ? itemVida : itemCrecimiento;
        StartCoroutine(Rebotar());
        if (item != null) StartCoroutine(SacarItem(item));
    }

    // La caja sube un poco y vuelve a su lugar
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

    // El objeto sale de la caja hacia arriba y se queda encima, listo para recogerse
    private IEnumerator SacarItem(GameObject item)
    {
        Vector3 inicio = transform.position;
        item.transform.position = inicio;
        item.SetActive(true);

        float t = 0f;
        while (t < duracionSalida && item != null)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / duracionSalida);
            item.transform.position = inicio + Vector3.up * (alturaSalidaItem * k);
            yield return null;
        }
    }
}
