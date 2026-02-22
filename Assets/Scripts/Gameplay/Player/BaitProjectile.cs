using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class BaitProjectile : MonoBehaviour
{
    [SerializeField] private float speed = 10f;
    [SerializeField] private float lifetime = 2f;
    [SerializeField] public float spinSpeed = 720f;
    [SerializeField] private LayerMask wallLayers;

    private Vector2 moveDirection = Vector2.down;
    private float lifeRemaining;
    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        // Default to project wall layers: 20, 21, 22 ("Layer 1/2/3")
        if (wallLayers.value == 0)
        {
            wallLayers = (1 << 20) | (1 << 21) | (1 << 22);
        }
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
        if (TryMarkEnemy(collision.gameObject))
        {
            Destroy(gameObject);
            return;
        }

        if (IsWallCollision(collision))
        {
            Destroy(gameObject);
        }
    }

    private bool TryMarkEnemy(GameObject target)
    {
        Component markable = target.GetComponent("MarkableEnemy");
        if (markable == null)
        {
            Transform current = target.transform.parent;
            while (current != null && markable == null)
            {
                markable = current.GetComponent("MarkableEnemy");
                current = current.parent;
            }
        }

        if (markable == null)
        {
            return false;
        }

        Debug.Log($"Bait hit: {target.name}");
        markable.SendMessage("Mark", SendMessageOptions.DontRequireReceiver);
        return true;
    }

    private bool IsWallCollision(Collision2D collision)
    {
        GameObject hit = collision.collider != null ? collision.collider.gameObject : collision.gameObject;
        if (IsInWallLayer(hit.layer))
        {
            return true;
        }

        Transform parent = hit.transform.parent;
        if (parent != null && IsInWallLayer(parent.gameObject.layer))
        {
            return true;
        }

        Transform root = hit.transform.root;
        return root != null && IsInWallLayer(root.gameObject.layer);
    }

    private bool IsInWallLayer(int layer)
    {
        return (wallLayers.value & (1 << layer)) != 0;
    }
}
