using UnityEngine;

// Retroalimentación visual: explosión de chispas cuadradas (estilo pixel) al recolectar, aplastar,
// recibir un golpe o crecer. Usa sprites simples en lugar de un sistema de partículas para que
// funcione igual con el renderizador 2D sin materiales extra.
public class Chispas : MonoBehaviour
{
    private Vector2 velocidad;
    private float vida;
    private float vidaTotal;
    private SpriteRenderer sr;

    private static Sprite pixel;

    public static void Explotar(Vector3 posicion, Color color, int cantidad = 10, float fuerza = 4f)
    {
        if (pixel == null) pixel = Resources.Load<Sprite>("UI/pixel");
        if (pixel == null) return;

        for (int i = 0; i < cantidad; i++)
        {
            GameObject go = new GameObject("Chispa");
            go.transform.position = posicion;
            float tam = Random.Range(0.6f, 1.2f);
            go.transform.localScale = new Vector3(tam, tam, 1f);

            Chispas c = go.AddComponent<Chispas>();
            c.sr = go.AddComponent<SpriteRenderer>();
            c.sr.sprite = pixel;
            c.sr.color = color;
            c.sr.sortingOrder = 20;

            float angulo = Random.Range(20f, 160f) * Mathf.Deg2Rad;
            c.velocidad = new Vector2(Mathf.Cos(angulo), Mathf.Sin(angulo)) * Random.Range(fuerza * 0.5f, fuerza);
            c.vidaTotal = Random.Range(0.35f, 0.6f);
            c.vida = c.vidaTotal;
        }
    }

    void Update()
    {
        vida -= Time.deltaTime;
        if (vida <= 0f)
        {
            Destroy(gameObject);
            return;
        }

        velocidad += Vector2.down * 12f * Time.deltaTime;
        transform.position += (Vector3)(velocidad * Time.deltaTime);

        Color c = sr.color;
        c.a = vida / vidaTotal;
        sr.color = c;
    }
}
