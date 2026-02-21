using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class BaitProjectile : MonoBehaviour
{
    [SerializeField] private float speed = 10f;
    [SerializeField] private float lifetime = 2f;
    [SerializeField] public float spinSpeed = 720f;

    private Vector2 moveDirection = Vector2.down;
    private float lifeRemaining;
    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void Initialize(Vector2 direction, float moveSpeed)
    {
        if (direction.sqrMagnitude > 0.0001f)
        {
            moveDirection = direction.normalized;
        }

        speed = moveSpeed;

        if (rb != null)
        {
            rb.linearVelocity = moveDirection * speed;
        }
    }

    private void OnEnable()
    {
        lifeRemaining = lifetime;
        if (rb != null)
        {
            rb.linearVelocity = moveDirection * speed;
        }
    }

    private void Update()
    {
        transform.Rotate(0f, 0f, spinSpeed * Time.deltaTime);

        lifeRemaining -= Time.deltaTime;
        if (lifeRemaining <= 0f)
        {
            Destroy(gameObject);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == 1)
        {
            Destroy(gameObject);
            return;
        }

        HandleHit(collision.gameObject);
    }

    private void HandleHit(GameObject target)
    {
        Component markable = target.GetComponent("MarkableEnemy");
        if (markable == null)
        {
            return;
        }

        Debug.Log($"Bait hit: {target.name}");
        markable.SendMessage("Mark", SendMessageOptions.DontRequireReceiver);
        Destroy(gameObject);
    }
}
