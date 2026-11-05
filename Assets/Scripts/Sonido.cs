using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Música de fondo y efectos de sonido. Se crea solo al iniciar el juego y sobrevive entre escenas,
// así que la música no se corta ni se encima al cambiar de escena.
// Los clips se cargan por nombre desde Resources/Audio/Musica y Resources/Audio/Efectos.
// Los volúmenes se guardan en PlayerPrefs (los cambia el menú de opciones).
public class Sonido : MonoBehaviour
{
    private const string ClaveMusica = "VolumenMusica";
    private const string ClaveEfectos = "VolumenEfectos";
    private const float DuracionCambio = 0.5f;

    private static Sonido instancia;

    private AudioSource fuenteMusica;
    private AudioSource fuenteEfectos;
    private readonly Dictionary<string, AudioClip> cache = new Dictionary<string, AudioClip>();
    private string pistaActual;
    private Coroutine cambioPista;

    public static float VolumenMusica
    {
        get => PlayerPrefs.GetFloat(ClaveMusica, 0.5f);
        set
        {
            PlayerPrefs.SetFloat(ClaveMusica, Mathf.Clamp01(value));
            Instancia.fuenteMusica.volume = Mathf.Clamp01(value);
        }
    }

    public static float VolumenEfectos
    {
        get => PlayerPrefs.GetFloat(ClaveEfectos, 0.8f);
        set
        {
            PlayerPrefs.SetFloat(ClaveEfectos, Mathf.Clamp01(value));
            Instancia.fuenteEfectos.volume = Mathf.Clamp01(value);
        }
    }

    private static Sonido Instancia
    {
        get
        {
            if (instancia == null) Crear();
            return instancia;
        }
    }

    // Se ejecuta al entrar a Play, aunque esté desactivado "Reload Domain"
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Crear()
    {
        if (instancia != null) return;

        GameObject go = new GameObject("Sonido");
        DontDestroyOnLoad(go);
        instancia = go.AddComponent<Sonido>();

        instancia.fuenteMusica = go.AddComponent<AudioSource>();
        instancia.fuenteMusica.loop = true;
        instancia.fuenteMusica.playOnAwake = false;
        instancia.fuenteMusica.volume = VolumenMusica;

        instancia.fuenteEfectos = go.AddComponent<AudioSource>();
        instancia.fuenteEfectos.playOnAwake = false;
        instancia.fuenteEfectos.volume = VolumenEfectos;
    }

    // Reproduce un efecto de Resources/Audio/Efectos (salto, fresa, golpe, aplastar, caja, crecer...)
    public static void Efecto(string nombre, float volumen = 1f)
    {
        AudioClip clip = Instancia.Cargar("Audio/Efectos/" + nombre);
        if (clip != null) Instancia.fuenteEfectos.PlayOneShot(clip, volumen);
    }

    // Cambia la música de fondo con un fundido corto; si ya suena esa pista no hace nada
    public static void Musica(string nombre)
    {
        if (Instancia.pistaActual == nombre && Instancia.fuenteMusica.isPlaying) return;

        AudioClip clip = Instancia.Cargar("Audio/Musica/" + nombre);
        if (clip == null) return;

        Instancia.pistaActual = nombre;
        if (Instancia.cambioPista != null) Instancia.StopCoroutine(Instancia.cambioPista);
        Instancia.cambioPista = Instancia.StartCoroutine(Instancia.CambiarPista(clip));
    }

    public static void DetenerMusica()
    {
        Instancia.pistaActual = null;
        if (Instancia.cambioPista != null) Instancia.StopCoroutine(Instancia.cambioPista);
        Instancia.fuenteMusica.Stop();
    }

    private IEnumerator CambiarPista(AudioClip clip)
    {
        // Baja la pista anterior, cambia y sube la nueva (tiempo sin escala: funciona en pausa)
        float inicio = fuenteMusica.volume;
        for (float t = 0f; t < DuracionCambio && fuenteMusica.isPlaying; t += Time.unscaledDeltaTime)
        {
            fuenteMusica.volume = Mathf.Lerp(inicio, 0f, t / DuracionCambio);
            yield return null;
        }

        fuenteMusica.clip = clip;
        fuenteMusica.Play();
        for (float t = 0f; t < DuracionCambio; t += Time.unscaledDeltaTime)
        {
            fuenteMusica.volume = Mathf.Lerp(0f, VolumenMusica, t / DuracionCambio);
            yield return null;
        }
        fuenteMusica.volume = VolumenMusica;
        cambioPista = null;
    }

    private AudioClip Cargar(string ruta)
    {
        if (!cache.TryGetValue(ruta, out AudioClip clip))
        {
            clip = Resources.Load<AudioClip>(ruta);
            if (clip == null) Debug.LogWarning("Sonido: no existe el audio Resources/" + ruta);
            cache[ruta] = clip;
        }
        return clip;
    }
}
