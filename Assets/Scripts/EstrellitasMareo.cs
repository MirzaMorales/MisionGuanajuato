using UnityEngine;

// Estrellitas de mareo sobre la cabeza del Estudiante derrotado (las crea PlayerHealth).
// Giran en óvalo alrededor del objeto; la que va "atrás" se ve más chica para dar profundidad.
// Usan tiempo real: siguen girando aunque Partida pause el juego al mostrar el Game Over
public class EstrellitasMareo : MonoBehaviour
{
    [SerializeField] private float radioX = 0.32f;
    [SerializeField] private float radioY = 0.1f;
    [SerializeField] private float vueltasPorSegundo = 0.8f;
    [SerializeField] private float giroPropio = 240f;

    void Update()
    {
        int n = transform.childCount;
        float angulo = Time.unscaledTime * vueltasPorSegundo * Mathf.PI * 2f;
        for (int i = 0; i < n; i++)
        {
            Transform estrella = transform.GetChild(i);
            float a = angulo + i * Mathf.PI * 2f / n;
            float profundidad = Mathf.Sin(a);   // -1 atrás, 1 adelante
            estrella.localPosition = new Vector3(Mathf.Cos(a) * radioX, profundidad * radioY, 0f);
            estrella.localScale = Vector3.one * Mathf.Lerp(0.7f, 1f, (profundidad + 1f) * 0.5f);
            estrella.localRotation = Quaternion.Euler(0f, 0f, Time.unscaledTime * giroPropio);
        }
    }
}
