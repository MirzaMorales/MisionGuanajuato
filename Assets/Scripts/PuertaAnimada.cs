using UnityEngine;

// Puerta con dos sprites (cerrada y abierta). La abre LevelGoal justo antes de cambiar de escena
public class PuertaAnimada : MonoBehaviour
{
    [SerializeField] private Sprite spriteAbierta;

    private SpriteRenderer sr;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    public void Abrir()
    {
        if (sr != null && spriteAbierta != null) sr.sprite = spriteAbierta;
    }
}
