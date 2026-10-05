using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private Animator animator;

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        // Por ahora solo leemos el teclado y avisamos al Animator.
        // El movimiento real se programa en HU01 y el salto en HU02.
        float horizontal = Input.GetAxisRaw("Horizontal");
        animator.SetFloat("Speed", Mathf.Abs(horizontal));
        animator.SetBool("IsJumping", Input.GetKey(KeyCode.Space));
    }
}