using UnityEngine;

// Comida que lanza el jugador (LanzadorComida). Vuela en arco; al tocar a Mr. Johnson se la come (BossController.Comer)
// y desaparece también si toca el piso o pasa su tiempo
public class ComidaProyectil : MonoBehaviour
{
    private const float Gravedad = -14f;

    private Vector2 velocidad;
    private float vidaRestante = 3f;

    public void Iniciar(Vector2 velocidadInicial)
    {
        velocidad = velocidadInicial;
    }

    void Update()
    {
        velocidad.y += Gravedad * Time.deltaTime;
        transform.position += (Vector3)(velocidad * Time.deltaTime);
        transform.Rotate(0f, 0f, -360f * Time.deltaTime);

        vidaRestante -= Time.deltaTime;
        if (vidaRestante <= 0f) Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D otro)
    {
        BossController jefe = otro.GetComponent<BossController>();
        if (jefe != null)
        {
            jefe.Comer();
            Destroy(gameObject);
            return;
        }

        // Capa 6 = Suelo
        if (otro.gameObject.layer == 6 && velocidad.y < 0f) Destroy(gameObject);
    }
}
