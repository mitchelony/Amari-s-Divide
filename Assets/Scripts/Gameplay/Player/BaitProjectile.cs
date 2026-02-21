using UnityEngine;

public class BaitProjectile : MonoBehaviour
{
    [SerializeField] private float speed = 10f;
    [SerializeField] private float lifetime = 2f;

    private Vector2 moveDirection = Vector2.down;
    private float lifeRemaining;

    public void Initialize(Vector2 direction, float moveSpeed)
    {
        if (direction.sqrMagnitude > 0.0001f)
        {
            moveDirection = direction.normalized;
        }

        speed = moveSpeed;
    }

    private void OnEnable()
    {
        lifeRemaining = lifetime;
    }

    private void Update()
    {
        transform.position += (Vector3)(moveDirection * speed * Time.deltaTime);

        lifeRemaining -= Time.deltaTime;
        if (lifeRemaining <= 0f)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        HandleHit(other.gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        HandleHit(collision.gameObject);
    }

    private void HandleHit(GameObject target)
    {
        Component markable = target.GetComponent("MarkableEnemy");
        bool isEnemy = markable != null || target.CompareTag("Enemy");

        if (!isEnemy)
        {
            return;
        }

        Debug.Log($"Bait hit: {target.name}");

        if (markable != null)
        {
            markable.SendMessage("Mark", SendMessageOptions.DontRequireReceiver);
        }

        Destroy(gameObject);
    }
}
