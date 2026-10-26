
using System.Collections;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [Header("Enemy Settings")]
    [SerializeField] private GameObject player;
    [SerializeField] private float speed = 2f;
    [SerializeField] private float distanceWakeUp = 4f;
    [SerializeField] private float distanceAttack = 1f;
    [SerializeField] private float distanceLoseTarget = 6f;
    [SerializeField] private int life = 3;

    private enum EnemyState
    {
        Idle,
        Chase,
        Attack,
        Return,
        Dead
    }

    private EnemyState state = EnemyState.Idle;

    private Vector3 initialPosition;

    private bool attacking;
    private bool invulnerable;

    private Animator animator;
    private SpriteRenderer spriteRender;
    private Collider2D collider2D;
    private Coroutine attackCoroutine;

    void Awake()
    {
        animator = GetComponent<Animator>();
        spriteRender = GetComponent<SpriteRenderer>();
        collider2D = GetComponent<Collider2D>();

        initialPosition = transform.position;
    }

    void Start()
    {
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player");
        }
    }

    void Update()
    {
        if (player == null || state == EnemyState.Dead)
        {
            AnimatorManager();
            return;
        }

        float distance = Vector2.Distance(
            transform.position,
            player.transform.position
        );

        float distanceFromStart = Vector2.Distance(
            initialPosition,
            player.transform.position
        );

        switch (state)
        {
            case EnemyState.Idle:

                if (distance <= distanceWakeUp)
                {
                    state = EnemyState.Chase;
                }

                break;

            case EnemyState.Chase:

                if (distanceFromStart > distanceLoseTarget)
                {
                    ReturnToStart();
                }
                else if (distance <= distanceAttack)
                {
                    state = EnemyState.Attack;
                }
                else
                {
                    Move(player.transform.position);
                }

                break;

            case EnemyState.Attack:

                if (distanceFromStart > distanceLoseTarget)
                {
                    ReturnToStart();
                }
                else if (distance > distanceAttack)
                {
                    StopAttack();
                    state = EnemyState.Chase;
                }
                else
                {
                    FaceTarget(player.transform.position);
                    Attack();
                }

                break;

            case EnemyState.Return:

                Move(initialPosition);

                if (Vector2.Distance(
                    transform.position,
                    initialPosition) <= 0.05f)
                {
                    transform.position = initialPosition;
                    state = EnemyState.Idle;
                }

                break;
        }

        AnimatorManager();
    }

    void Move(Vector3 target)
    {
        transform.position = Vector3.MoveTowards(
            transform.position,
            target,
            speed * Time.deltaTime
        );

        FaceTarget(target);
    }

    void FaceTarget(Vector3 target)
    {
        if (Mathf.Abs(target.x - transform.position.x) > 0.01f)
        {
            spriteRender.flipX = target.x > transform.position.x;
        }
    }

    void Attack()
    {
        if (!attacking)
        {
            attackCoroutine = StartCoroutine(Attacking());
        }
    }

    IEnumerator Attacking()
    {
        attacking = true;

        yield return new WaitForSeconds(0.5f);

        if (player != null && state == EnemyState.Attack)
        {
            float distance = Vector2.Distance(
                transform.position,
                player.transform.position
            );

            if (distance <= distanceAttack)
            {
                PlayerControls playerControls =
                    player.GetComponent<PlayerControls>();

                if (playerControls != null)
                {
                    playerControls.TakeDamage(1);
                }
            }
        }

        yield return new WaitForSeconds(1f);

        attacking = false;
        attackCoroutine = null;
    }

    void StopAttack()
    {
        if (attackCoroutine != null)
        {
            StopCoroutine(attackCoroutine);
            attackCoroutine = null;
        }

        attacking = false;
    }

    void ReturnToStart()
    {
        StopAttack();
        state = EnemyState.Return;
    }

    void AnimatorManager()
    {
        animator.SetBool(
            "pWakeUp",
            state == EnemyState.Chase ||
            state == EnemyState.Attack ||
            state == EnemyState.Return
        );

        animator.SetBool("pAttack", attacking);
        animator.SetBool("pDeath", state == EnemyState.Dead);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (state == EnemyState.Dead || invulnerable)
            return;

        if (other.CompareTag("Punch"))
        {
            invulnerable = true;
            StartCoroutine(Damage());
        }
    }

    IEnumerator Damage()
    {
        life--;

        Debug.Log("Vida do inimigo: " + life);

        if (life <= 0)
        {
            state = EnemyState.Dead;

            StopAttack();

            collider2D.enabled = false;

            animator.SetBool("pDeath", true);

            yield return new WaitForSeconds(1.0f);

            Destroy(gameObject);
        }
        else
        {
            spriteRender.color = Color.red;

            yield return new WaitForSeconds(0.25f);

            spriteRender.color = Color.white;
            invulnerable = false;
        }
    }
}
