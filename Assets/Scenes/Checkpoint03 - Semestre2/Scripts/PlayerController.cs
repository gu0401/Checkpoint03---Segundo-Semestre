
using System.Collections;
using UnityEngine;

public class PlayerControls : MonoBehaviour
{
    [Header("Move Settings")]
    [SerializeField] private float speed = 4f;

    [Header("Attack Settings")]
    [SerializeField] private GameObject attackObject;
    [SerializeField] private float attackDelay = 0.3f;
    [SerializeField] private float attackDuration = 0.2f;

    [Header("Life Settings")]
    [SerializeField] private int life = 3;

    private Vector2 direction;
    private bool attacking;
    private bool dead;
    private bool invulnerable;

    private Rigidbody2D rigidbody2D;
    private Animator animator;
    private Collider2D attackCollider;
    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        rigidbody2D = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        attackCollider = attackObject.GetComponent<Collider2D>();
        attackCollider.enabled = false;
    }

    void Start()
    {
        rigidbody2D.gravityScale = 0f;
        rigidbody2D.freezeRotation = true;
    }

    void Update()
    {
        if (dead) return;

        Move();
        Attack();
        Animations();
    }

    void FixedUpdate()
    {
        if (dead) return;

        OnMove();
    }

    void Move()
    {
        direction = new Vector2(
            Input.GetAxisRaw("Horizontal"),
            Input.GetAxisRaw("Vertical")
        );

        if (direction.x != 0)
        {
            Vector3 scale = transform.localScale;
            scale.x = Mathf.Abs(scale.x) *
                      (direction.x > 0 ? 1f : -1f);

            transform.localScale = scale;
        }
    }

    void OnMove()
    {
        rigidbody2D.linearVelocity = direction.normalized * speed;
    }

    void Attack()
    {
        if (Input.GetButtonDown("Fire1") && !attacking)
        {
            StartCoroutine(OnAttack());
        }
    }

    IEnumerator OnAttack()
    {
        attacking = true;

        yield return new WaitForSeconds(attackDelay);

        attackCollider.enabled = true;

        yield return new WaitForSeconds(attackDuration);

        attackCollider.enabled = false;
        attacking = false;
    }

    public void TakeDamage(int damage)
    {
        if (dead || invulnerable) return;

        life -= damage;

        Debug.Log("Vida do jogador: " + life);

        if (life <= 0)
        {
            Die();
        }
        else
        {
            StartCoroutine(DamageFeedback());
        }
    }

    IEnumerator DamageFeedback()
    {
        invulnerable = true;

        spriteRenderer.color = Color.red;

        yield return new WaitForSeconds(0.5f);

        spriteRenderer.color = Color.white;
        invulnerable = false;
    }

    void Die()
    {
        dead = true;
        direction = Vector2.zero;

        StopAllCoroutines();

        rigidbody2D.linearVelocity = Vector2.zero;
        attackCollider.enabled = false;

        animator.SetBool("pAttack", false);
        animator.SetInteger("pMove", 0);

        spriteRenderer.color = Color.gray;

        Destroy(gameObject);
    }

    void Animations()
    {
        animator.SetInteger(
            "pMove",
            direction.sqrMagnitude > 0 ? 1 : 0
        );

        animator.SetBool("pAttack", attacking);
    }
}
