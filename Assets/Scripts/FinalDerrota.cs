using System.Collections;
using UnityEngine;

// Final triste (HU18-RF02): si el jugador pierde todas sus vidas contra Mr. Johnson, antes del Game Over se ve esto solo:
// la novia llora y se cae de rodillas, Doña María se pone feliz, Mr. Johnson se voltea, camina hacia la novia,
// se arrodilla y le ofrece un anillo mientras ella sigue llorando
public class FinalDerrota : MonoBehaviour
{
    [Header("Mr. Johnson")]
    [SerializeField] private BossController jefe;
    [SerializeField] private Sprite jefeParado;
    [SerializeField] private Sprite jefeCaminar;
    [SerializeField] private Sprite[] jefeArrodillado;
    [SerializeField] private float velocidadJefe = 2.2f;
    // A qué distancia de la novia se arrodilla
    [SerializeField] private float distanciaALaNovia = 1.4f;

    [Header("Doña María y la novia")]
    [SerializeField] private SpriteRenderer madre;
    [SerializeField] private Sprite[] madreFeliz;
    [SerializeField] private SpriteRenderer novia;
    // 0: llorando de pie, 1: cayendo de rodillas, 2: sentada llorando
    [SerializeField] private Sprite[] noviaLlorando;

    [SerializeField] private float segundosDeEscena = 5f;

    void OnEnable()
    {
        Partida.EscenaDeDerrota = Secuencia;
    }

    void OnDisable()
    {
        if (Partida.EscenaDeDerrota == (System.Func<IEnumerator>)Secuencia) Partida.EscenaDeDerrota = null;
    }

    private IEnumerator Secuencia()
    {
        SpriteRenderer srJefe = jefe.GetComponent<SpriteRenderer>();
        jefe.Triunfar();
        yield return new WaitForSeconds(1.2f);

        // Doña María deja de jalar a la novia y se pone feliz; la novia llora
        madre.sprite = madreFeliz[0];
        novia.gameObject.SetActive(true);
        // Los cuadros de llanto tienen el pivote en los pies: se pone a la novia al nivel del piso, donde está Doña María
        novia.transform.position = new Vector3(novia.transform.position.x, madre.transform.position.y, 0f);
        novia.sprite = noviaLlorando[0];
        Sonido.Efecto("golpe", 0.6f);
        yield return new WaitForSeconds(0.9f);

        // La novia se cae de rodillas llorando
        novia.sprite = noviaLlorando[1];
        Chispas.Explotar(novia.transform.position + Vector3.up * 0.1f, new Color(0.8f, 0.75f, 0.7f), 8, 2f);
        Sonido.Efecto("aplastar", 0.5f);
        yield return new WaitForSeconds(0.8f);

        // Mr. Johnson se voltea hacia ellas
        srJefe.sprite = jefeParado;
        srJefe.flipX = true;
        yield return new WaitForSeconds(0.7f);

        // Camina hasta ponerse frente a la novia
        float destino = novia.transform.position.x - distanciaALaNovia;
        float t = 0f;
        while (jefe.transform.position.x < destino)
        {
            t += Time.deltaTime;
            srJefe.sprite = (int)(t * 4f) % 2 == 0 ? jefeParado : jefeCaminar;
            jefe.transform.position += Vector3.right * velocidadJefe * Time.deltaTime;
            AnimarMadreYNovia(t);
            yield return null;
        }

        // Se arrodilla y le da el anillo mientras ella llora y la mamá celebra
        srJefe.flipX = false;
        Sonido.Efecto("checkpoint");
        for (float e = 0f; e < segundosDeEscena; e += Time.deltaTime)
        {
            srJefe.sprite = jefeArrodillado[(int)(e * 3f) % jefeArrodillado.Length];
            AnimarMadreYNovia(e);
            if (Random.value < 0.05f) Chispas.Explotar(jefe.transform.position + new Vector3(1.1f, 0.9f, 0f), new Color(1f, 0.95f, 0.5f), 5, 2f);
            yield return null;
        }
    }

    private void AnimarMadreYNovia(float tiempo)
    {
        madre.sprite = madreFeliz[(int)(tiempo * 3f) % madreFeliz.Length];
        // Sentada llorando: alterna entre las dos poses de llanto y se mece un poco
        novia.sprite = noviaLlorando[(int)(tiempo * 2.5f) % 2 == 0 ? 2 : 1];
    }
}
