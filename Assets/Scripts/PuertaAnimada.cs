using System.Collections;
using UnityEngine;

// Puerta con sprites de cerrada (el del SpriteRenderer), uno intermedio opcional y abierta. La abre LevelGoal justo antes
// de cambiar de escena: si tiene cuadro intermedio se ve cómo se va abriendo
public class PuertaAnimada : MonoBehaviour
{
    [SerializeField] private Sprite spriteMedia;
    [SerializeField] private Sprite spriteAbierta;
    [SerializeField] private float segundosPorCuadro = 0.2f;

    private SpriteRenderer sr;
    private Sprite spriteCerrada;
    private bool abierta;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        if (sr != null) spriteCerrada = sr.sprite;
    }

    public void Abrir()
    {
        if (sr == null || spriteAbierta == null || abierta) return;
        abierta = true;
        Sonido.Efecto("puerta");

        if (spriteMedia != null) StartCoroutine(Secuencia());
        else sr.sprite = spriteAbierta;
    }

    // Vuelve a dejar la puerta cerrada (la de la intro se cierra después de que pasan)
    public void Cerrar()
    {
        StopAllCoroutines();
        if (sr != null && spriteCerrada != null) sr.sprite = spriteCerrada;
        abierta = false;
    }

    private IEnumerator Secuencia()
    {
        sr.sprite = spriteMedia;
        yield return new WaitForSeconds(segundosPorCuadro);
        sr.sprite = spriteAbierta;
    }
}
