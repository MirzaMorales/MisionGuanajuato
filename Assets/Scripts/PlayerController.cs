using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float velocidad = 5f;
    [SerializeField] private float fuerzaSalto = 12f;

    [Header("Detección de suelo")]
    [SerializeField] private Transform verificadorSuelo;
    [SerializeField] private float radioSuelo = 0.1f;
    [SerializeField] private LayerMask capaSuelo;

    private Rigidbody2D rb;
    private Animator animator;
    private float horizontal;
    private bool enSuelo;
    private bool saltoPedido;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        horizontal = Input.GetAxisRaw("Horizontal");

        // ¿Hay suelo debajo de los pies?
        enSuelo = Physics2D.OverlapCircle(verificadorSuelo.position, radioSuelo, capaSuelo);

        // Solo se puede saltar desde el suelo (esto evita el doble salto)
        if (Input.GetButtonDown("Jump") && enSuelo)
        {
            saltoPedido = true;
        }

        animator.SetFloat("Speed", Mathf.Abs(horizontal));
        animator.SetBool("IsJumping", !enSuelo);
    }

    void FixedUpdate()
    {
        rb.linearVelocity = new Vector2(horizontal * velocidad, rb.linearVelocity.y);

        if (saltoPedido)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, fuerzaSalto);
            saltoPedido = false;
        }
    }

    // Dibuja el círculo del detector en la pestaña Scene al seleccionar al personaje
    void OnDrawGizmosSelected()
    {
        if (verificadorSuelo == null) return;
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(verificadorSuelo.position, radioSuelo);
    }
}