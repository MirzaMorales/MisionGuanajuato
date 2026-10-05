using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // Aparece en el Inspector para poder cambiarla sin tocar el código
    [SerializeField] private float velocidad = 5f;

    private Rigidbody2D rb;
    private Animator animator;
    private float horizontal;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        // A/D o flechas: -1 izquierda, 0 quieto, 1 derecha
        horizontal = Input.GetAxisRaw("Horizontal");

        animator.SetFloat("Speed", Mathf.Abs(horizontal));
        // Temporal: se reemplaza en HU02 (salto)
        animator.SetBool("IsJumping", Input.GetKey( KeyCode.Space));
    }

    void FixedUpdate()
    {
        // Mueve en horizontal y respeta la velocidad vertical (gravedad)
        rb.linearVelocity = new Vector2(horizontal * velocidad, rb.linearVelocity.y);
    }
}