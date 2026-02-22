using System;
using System.Reflection;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class EnemyController : MonoBehaviour, IDamageable
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2.5f;
    [SerializeField] private float stoppingDistance = 0.05f;

    [Header("Combat")]
    [SerializeField] private int maxHealth = 3;
    [SerializeField] private int contactDamage = 1;
    [SerializeField] private float contactDamageCooldown = 0.75f;

    private Rigidbody2D rb;
    private Animator animator;
    private Collider2D bodyCollider;
    private Transform player;
    private Vector2 desiredMove;
    private Vector2 facing = Vector2.down;
    private float nextDamageTime;
    private int currentHealth;
    private bool warnedMissingPlayerDamageReceiver;

    private bool hasMoveX;
    private bool hasMoveY;
    private bool hasIsMoving;
    private bool hasDirection;
    private bool hasSpumMove;
    private bool hasSpumAttack;
    private bool hasAttack;

    private int moveXHash;
    private int moveYHash;
    private int isMovingHash;
    private int directionHash;
    private int spumMoveHash;
    private int spumAttackHash;
    private int attackHash;

    protected Transform PlayerTransform => player;
    protected Rigidbody2D Body => rb;
    protected Vector2 DesiredMove => desiredMove;
    protected float MoveSpeed => moveSpeed;
    protected float StoppingDistance => stoppingDistance;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        bodyCollider = GetComponent<Collider2D>();
        animator = GetComponentInChildren<Animator>();

        if (rb != null)
        {
            rb.gravityScale = 0f;
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.freezeRotation = true;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        }

        if (bodyCollider != null)
        {
            bodyCollider.isTrigger = false;
        }

        currentHealth = Mathf.Max(1, maxHealth);
        CacheAnimatorParameters();
        TryFindPlayer();
    }

    protected virtual void OnEnable()
    {
        currentHealth = Mathf.Max(1, maxHealth);
        nextDamageTime = 0f;
        warnedMissingPlayerDamageReceiver = false;
        desiredMove = Vector2.zero;

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
    }

    protected virtual void Update()
    {
        if (player == null)
        {
            TryFindPlayer();
        }

        UpdateAnimator();
    }

    protected virtual void FixedUpdate()
    {
        desiredMove = ComputeDesiredMove();
        if (rb == null)
        {
            return;
        }

        rb.MovePosition(rb.position + desiredMove * Time.fixedDeltaTime);

        if (desiredMove.sqrMagnitude > 0.0001f)
        {
            facing = desiredMove.normalized;
        }
    }

    protected virtual Vector2 ComputeDesiredMove()
    {
        if (player == null || rb == null)
        {
            return Vector2.zero;
        }

        Vector2 toPlayer = (Vector2)player.position - rb.position;
        float distance = toPlayer.magnitude;
        if (distance <= stoppingDistance)
        {
            return Vector2.zero;
        }

        return toPlayer.normalized * moveSpeed;
    }

    private void TryFindPlayer()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            player = playerObject.transform;
        }
    }

    private void CacheAnimatorParameters()
    {
        moveXHash = Animator.StringToHash("MoveX");
        moveYHash = Animator.StringToHash("MoveY");
        isMovingHash = Animator.StringToHash("IsMoving");
        directionHash = Animator.StringToHash("Direction");
        spumMoveHash = Animator.StringToHash("1_Move");
        spumAttackHash = Animator.StringToHash("2_Attack");
        attackHash = Animator.StringToHash("Attack");

        if (animator == null)
        {
            return;
        }

        foreach (AnimatorControllerParameter parameter in animator.parameters)
        {
            if (parameter.name == "MoveX" && parameter.type == AnimatorControllerParameterType.Float)
            {
                hasMoveX = true;
            }
            else if (parameter.name == "MoveY" && parameter.type == AnimatorControllerParameterType.Float)
            {
                hasMoveY = true;
            }
            else if (parameter.name == "IsMoving" && parameter.type == AnimatorControllerParameterType.Bool)
            {
                hasIsMoving = true;
            }
            else if (parameter.name == "Direction" && parameter.type == AnimatorControllerParameterType.Int)
            {
                hasDirection = true;
            }
            else if (parameter.name == "1_Move" && parameter.type == AnimatorControllerParameterType.Bool)
            {
                hasSpumMove = true;
            }
            else if (parameter.name == "2_Attack" && parameter.type == AnimatorControllerParameterType.Trigger)
            {
                hasSpumAttack = true;
            }
            else if (parameter.name == "Attack" && parameter.type == AnimatorControllerParameterType.Trigger)
            {
                hasAttack = true;
            }
        }
    }

    private void UpdateAnimator()
    {
        if (animator == null)
        {
            return;
        }

        Vector2 direction = desiredMove.sqrMagnitude > 0.0001f ? desiredMove.normalized : facing;
        bool isMoving = desiredMove.sqrMagnitude > 0.0001f;

        if (hasMoveX)
        {
            animator.SetFloat(moveXHash, direction.x);
        }

        if (hasMoveY)
        {
            animator.SetFloat(moveYHash, direction.y);
        }

        if (hasIsMoving)
        {
            animator.SetBool(isMovingHash, isMoving);
        }

        if (hasDirection)
        {
            animator.SetInteger(directionHash, DirectionToInt(direction));
        }

        if (hasSpumMove)
        {
            animator.SetBool(spumMoveHash, isMoving);
        }
    }

    private int DirectionToInt(Vector2 direction)
    {
        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
        {
            return direction.x >= 0f ? 2 : 3;
        }

        return direction.y >= 0f ? 1 : 0;
    }

    protected void TriggerAttackAnimation()
    {
        if (animator == null)
        {
            return;
        }

        if (hasSpumAttack)
        {
            animator.SetTrigger(spumAttackHash);
            return;
        }

        if (hasAttack)
        {
            animator.SetTrigger(attackHash);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        TryDealContactDamage(collision.collider);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        TryDealContactDamage(collision.collider);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryDealContactDamage(other);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        TryDealContactDamage(other);
    }

    private void TryDealContactDamage(Collider2D other)
    {
        if (other == null || Time.time < nextDamageTime)
        {
            return;
        }

        Transform root = other.transform.root;
        if (root == null || !root.CompareTag("Player"))
        {
            return;
        }

        bool didDamage = TryInvokeDamage(root.gameObject, contactDamage);
        if (!didDamage && !warnedMissingPlayerDamageReceiver)
        {
            warnedMissingPlayerDamageReceiver = true;
            Debug.LogWarning("EnemyController could not find a player damage receiver (IDamageable or TakeDamage(int)).");
        }

        TriggerAttackAnimation();
        nextDamageTime = Time.time + contactDamageCooldown;
    }

    private bool TryInvokeDamage(GameObject target, int amount)
    {
        IDamageable damageable = target.GetComponentInParent<IDamageable>();
        if (damageable != null)
        {
            damageable.TakeDamage(amount);
            return true;
        }

        MonoBehaviour[] components = target.GetComponentsInChildren<MonoBehaviour>(true);
        for (int i = 0; i < components.Length; i++)
        {
            MonoBehaviour component = components[i];
            if (component == null)
            {
                continue;
            }

            MethodInfo method = component.GetType().GetMethod("TakeDamage", BindingFlags.Instance | BindingFlags.Public);
            if (method == null)
            {
                continue;
            }

            ParameterInfo[] parameters = method.GetParameters();
            if (parameters.Length == 1 && parameters[0].ParameterType == typeof(int))
            {
                method.Invoke(component, new object[] { amount });
                return true;
            }
        }

        return false;
    }

    public virtual void TakeDamage(int amount)
    {
        int damage = Mathf.Max(1, amount);
        currentHealth -= damage;

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    protected virtual void Die()
    {
        MarkableEnemy markable = GetComponent<MarkableEnemy>();
        if (markable != null && markable.IsMarked)
        {
            markable.Unmark();
        }

        EnemyPool.Despawn(gameObject);
    }
}
