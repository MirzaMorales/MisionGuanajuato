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
    private bool abierta;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    public void Abrir()
    {
        if (sr == null || spriteAbierta == null || abierta) return;
        abierta = true;
        Sonido.Efecto("puerta");

        if (spriteMedia != null) StartCoroutine(Secuencia());
        else sr.sprite = spriteAbierta;
    }

    private IEnumerator Secuencia()
    {
        sr.sprite = spriteMedia;
        yield return new WaitForSeconds(segundosPorCuadro);
        sr.sprite = spriteAbierta;
    }
}
