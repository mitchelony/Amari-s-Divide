using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
public class PetController : MonoBehaviour
{
    public Transform player;
    public float followDistance = 1.2f;
    public float followSpeed = 6f;
    public float chaseSpeed = 9f;
    public int contactDamage = 1;
    public float damageCooldown = 0.25f;
    public bool allowChaseCleave = true;

    [Header("Optional Hook")]
    [SerializeField] private bool chasePlayerWhenMarked = false;
    [SerializeField] private bool ignorePlayerCollision = true;

    private Rigidbody2D rb;
    private Animator animator;
    private PlayerStatus playerStatus;
    private Collider2D[] petColliders;
    private bool playerCollisionIgnored;

    private Vector2 desiredMove;
    private Vector2 facingDirection = Vector2.down;
    private float nextDamageTime;

    private bool hasMoveX;
    private bool hasMoveY;
    private bool hasIsMoving;
    private bool hasDirectionInt;

    private int moveXHash;
    private int moveYHash;
    private int isMovingHash;
    private int attackHash;
    private int directionHash;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        petColliders = GetComponentsInChildren<Collider2D>(true);

        if (player == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
            {
                player = playerObject.transform;
                playerStatus = playerObject.GetComponent<PlayerStatus>();
            }
        }
        else
        {
            playerStatus = player.GetComponent<PlayerStatus>();
        }

        CacheAnimatorParameters();
        ConfigurePhysicsBody();
    }

    private void Start()
    {
        TryConfigurePlayerCollisionIgnore();
    }

    private void Update()
    {
        if (!playerCollisionIgnored && ignorePlayerCollision)
        {
            TryConfigurePlayerCollisionIgnore();
        }

        UpdateAnimator();
    }

    private void FixedUpdate()
    {
        desiredMove = ComputeDesiredMove();

        Vector2 newPosition = rb.position + desiredMove * Time.fixedDeltaTime;
        rb.MovePosition(newPosition);

        if (desiredMove.sqrMagnitude > 0.0001f)
        {
            facingDirection = desiredMove.normalized;
        }
    }

    private Vector2 ComputeDesiredMove()
    {
        if (player == null)
        {
            return Vector2.zero;
        }

        if (chasePlayerWhenMarked && playerStatus != null && playerStatus.IsMarked)
        {
            return MoveToward(player.position, chaseSpeed);
        }

        MarkableEnemy markedEnemy = MarkableEnemy.Current;
        if (markedEnemy != null)
        {
            return MoveToward(markedEnemy.transform.position, chaseSpeed);
        }

        Vector2 toPlayer = (Vector2)player.position - rb.position;
        float distance = toPlayer.magnitude;

        if (distance <= followDistance)
        {
            return Vector2.zero;
        }

        return toPlayer.normalized * followSpeed;
    }

    private Vector2 MoveToward(Vector3 worldPosition, float speed)
    {
        Vector2 toTarget = (Vector2)worldPosition - rb.position;
        if (toTarget.sqrMagnitude <= 0.0001f)
        {
            return Vector2.zero;
        }

        return toTarget.normalized * speed;
    }

    private void CacheAnimatorParameters()
    {
        moveXHash = Animator.StringToHash("MoveX");
        moveYHash = Animator.StringToHash("MoveY");
        isMovingHash = Animator.StringToHash("IsMoving");
        attackHash = Animator.StringToHash("Attack");
        directionHash = Animator.StringToHash("Direction");

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
            else if (parameter.name == "Attack" && parameter.type == AnimatorControllerParameterType.Trigger)
            {
                // Present in current controller; trigger is used directly in TriggerAttack().
            }
            else if (parameter.name == "Direction" && parameter.type == AnimatorControllerParameterType.Int)
            {
                hasDirectionInt = true;
            }
        }
    }

    private void ConfigurePhysicsBody()
    {
        if (rb == null)
        {
            return;
        }

        rb.gravityScale = 0f;
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
    }

    private void TryConfigurePlayerCollisionIgnore()
    {
        if (!ignorePlayerCollision || player == null || petColliders == null || petColliders.Length == 0)
        {
            return;
        }

        Collider2D[] playerColliders = player.GetComponentsInChildren<Collider2D>(true);
        if (playerColliders == null || playerColliders.Length == 0)
        {
            return;
        }

        for (int i = 0; i < petColliders.Length; i++)
        {
            Collider2D petCollider = petColliders[i];
            if (petCollider == null)
            {
                continue;
            }

            for (int j = 0; j < playerColliders.Length; j++)
            {
                Collider2D playerCollider = playerColliders[j];
                if (playerCollider == null)
                {
                    continue;
                }

                Physics2D.IgnoreCollision(petCollider, playerCollider, true);
            }
        }

        playerCollisionIgnored = true;
    }

    private void UpdateAnimator()
    {
        Vector2 direction = desiredMove.sqrMagnitude > 0.0001f ? desiredMove.normalized : facingDirection;
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

        if (hasDirectionInt)
        {
            animator.SetInteger(directionHash, DirectionToInt(direction));
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
        if (other == null)
        {
            return;
        }

        MarkableEnemy marked = MarkableEnemy.Current;
        if (marked == null)
        {
            return;
        }

        MarkableEnemy hitMarked = other.GetComponentInParent<MarkableEnemy>();
        bool hitCurrentMarked = hitMarked != null && hitMarked == marked;
        if (!hitCurrentMarked && !allowChaseCleave)
        {
            return;
        }

        if (Time.time < nextDamageTime)
        {
            return;
        }

        if (hitCurrentMarked)
        {
            TriggerAttack();
        }

        IDamageable damageable = other.GetComponentInParent<IDamageable>();
        if (damageable == null)
        {
            return;
        }

        damageable.TakeDamage(contactDamage);
        nextDamageTime = Time.time + damageCooldown;
    }

    private void TriggerAttack()
    {
        animator.SetTrigger(attackHash);
    }
}
