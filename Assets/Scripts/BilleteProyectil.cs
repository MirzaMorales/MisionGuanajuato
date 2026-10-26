using UnityEngine;

// Billete que lanza Mr. Johnson. Lo crea BossController: vuela en línea recta girando y desaparece al pasar su tiempo.
// Lleva el tag Enemigo y un trigger, así que PlayerHealth le quita una vida al jugador que lo toque
public class BilleteProyectil : MonoBehaviour
{
    private Vector2 velocidad;
    private Sprite[] cuadros;
    private float cuadrosPorSegundo;
    private float vidaRestante;
    private SpriteRenderer sr;
    private float relojCuadro;
    private int cuadro;

    public void Iniciar(Vector2 velocidad, Sprite[] cuadros, float cuadrosPorSegundo, float segundosDeVida)
    {
        this.velocidad = velocidad;
        this.cuadros = cuadros;
        this.cuadrosPorSegundo = cuadrosPorSegundo;
        vidaRestante = segundosDeVida;
        sr = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        transform.position += (Vector3)(velocidad * Time.deltaTime);
        vidaRestante -= Time.deltaTime;
        if (vidaRestante <= 0f)
        {
            Destroy(gameObject);
            return;
        }

        if (sr == null || cuadros == null || cuadros.Length == 0) return;
        relojCuadro += Time.deltaTime;
        if (relojCuadro >= 1f / cuadrosPorSegundo)
        {
            relojCuadro = 0f;
            cuadro = (cuadro + 1) % cuadros.Length;
            sr.sprite = cuadros[cuadro];
        }
    }
}
