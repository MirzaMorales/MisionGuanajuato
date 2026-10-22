using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Fundido a negro al cambiar de escena. Se crea solo al iniciar el juego y sobrevive entre escenas,
// así que no hay que ponerlo en ninguna escena: basta con llamar a TransicionEscena.Cargar(...)
public class TransicionEscena : MonoBehaviour
{
    private const float DuracionPorDefecto = 0.6f;

    private static TransicionEscena instancia;

    private CanvasGroup velo;
    private bool enCurso;

    // Se ejecuta al entrar a Play, aunque esté desactivado "Reload Domain"
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Crear()
    {
        if (instancia != null) return;

        GameObject go = new GameObject("TransicionEscena");
        DontDestroyOnLoad(go);
        instancia = go.AddComponent<TransicionEscena>();
        instancia.ConstruirVelo();
    }

    private void ConstruirVelo()
    {
        // Canvas de pantalla completa por encima de todo, con un rectángulo negro
        Canvas canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 32000;

        velo = gameObject.AddComponent<CanvasGroup>();
        velo.alpha = 0f;
        velo.blocksRaycasts = false;
        velo.interactable = false;

        GameObject fondo = new GameObject("Negro");
        fondo.transform.SetParent(transform, false);
        Image imagen = fondo.AddComponent<Image>();
        imagen.color = Color.black;
        RectTransform rt = imagen.rectTransform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    // Funde a negro, carga la escena por nombre y funde de vuelta
    public static void Cargar(string nombreEscena, float duracion = DuracionPorDefecto)
    {
        Crear();
        if (instancia.enCurso) return;
        instancia.StartCoroutine(instancia.Rutina(nombreEscena, -1, duracion));
    }

    // Igual, pero por índice de Build Settings
    public static void Cargar(int indiceEscena, float duracion = DuracionPorDefecto)
    {
        Crear();
        if (instancia.enCurso) return;
        instancia.StartCoroutine(instancia.Rutina(null, indiceEscena, duracion));
    }

    private IEnumerator Rutina(string nombre, int indice, float duracion)
    {
        enCurso = true;
        velo.blocksRaycasts = true;

        yield return Fundir(0f, 1f, duracion);

        AsyncOperation carga = nombre != null
            ? SceneManager.LoadSceneAsync(nombre)
            : SceneManager.LoadSceneAsync(indice);
        while (!carga.isDone) yield return null;

        yield return Fundir(1f, 0f, duracion);

        velo.blocksRaycasts = false;
        enCurso = false;
    }

    // Tiempo sin escala: la transición funciona aunque el juego esté en pausa
    private IEnumerator Fundir(float desde, float hasta, float duracion)
    {
        float t = 0f;
        while (t < duracion)
        {
            t += Time.unscaledDeltaTime;
            velo.alpha = Mathf.Lerp(desde, hasta, t / duracion);
            yield return null;
        }
        velo.alpha = hasta;
    }
}
