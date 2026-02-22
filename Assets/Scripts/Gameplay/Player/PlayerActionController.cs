using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public class PlayerActionController : MonoBehaviour
{
    [Header("Collision Safety")]
    [SerializeField] private bool enforceCollisionSafety = true;

    [Header("Dash")]
    [SerializeField] private float dashSpeed = 12f;
    [SerializeField] private float dashDuration = 0.12f;
    [SerializeField] private float dashCooldown = 1.0f;

    [Header("Bait")]
    [SerializeField] private GameObject baitProjectilePrefab;
    [SerializeField] private float baitSpeed = 10f;
    [SerializeField] private float baitCooldown = 1.0f;
    [SerializeField] private float baitSpawnOffset = 0.6f;
    [SerializeField] private float baitVerticalOffset = 0.3f;

    private Rigidbody2D rb;
    private PlayerStatus playerStatus;
    private Behaviour movementController;
    private Vector2 facingDirection = Vector2.down;
    private float dashReadyTime;
    private float baitReadyTime;
    private bool isDashing;

    public float DashCooldownRemaining => Mathf.Max(0f, dashReadyTime - Time.time);
    public float BaitCooldownRemaining => Mathf.Max(0f, baitReadyTime - Time.time);

    private void Awake()
    {
        PlayerActionController[] controllers = GetComponents<PlayerActionController>();
        if (controllers.Length > 1 && controllers[0] != this)
        {
            enabled = false;
            return;
        }

        rb = GetComponent<Rigidbody2D>();
        playerStatus = GetComponent<PlayerStatus>();
        movementController = GetComponent("TopDownCharacterController") as Behaviour;

        if (enforceCollisionSafety)
        {
            ConfigureCollisionSafety();
        }
    }

    private void Update()
    {
        UpdateFacingDirection();

        if (Input.GetKeyDown(KeyCode.Space))
        {
            TryDash();
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            TryThrowBait();
        }
    }

    private void FixedUpdate()
    {
        if (isDashing && rb != null)
        {
            rb.linearVelocity = facingDirection * dashSpeed;
        }
    }

    private void UpdateFacingDirection()
    {
        if (rb == null)
        {
            return;
        }

        Vector2 velocity = rb.linearVelocity;
        if (velocity.sqrMagnitude > 0.0001f)
        {
            facingDirection = velocity.normalized;
        }
    }

    private void TryDash()
    {
        if (isDashing || Time.time < dashReadyTime)
        {
            return;
        }

        StartCoroutine(DashRoutine());
    }

    private IEnumerator DashRoutine()
    {
        isDashing = true;
        dashReadyTime = Time.time + dashCooldown;

        if (movementController != null)
        {
            movementController.enabled = false;
        }

        float elapsed = 0f;
        while (elapsed < dashDuration)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }

        if (movementController != null)
        {
            movementController.enabled = true;
        }

        isDashing = false;
    }

    private void TryThrowBait()
    {
        if (Time.time < baitReadyTime)
        {
            return;
        }

        if (playerStatus != null && playerStatus.IsMarked)
        {
            return;
        }

        if (baitProjectilePrefab == null)
        {
            return;
        }

        Vector2 spawnPosition = (Vector2)transform.position
            + (facingDirection.normalized * baitSpawnOffset)
            + (Vector2.up * baitVerticalOffset);
        GameObject instance = Instantiate(baitProjectilePrefab, spawnPosition, Quaternion.identity);
        BaitProjectile projectile = instance.GetComponent<BaitProjectile>();
        if (projectile != null)
        {
            projectile.Initialize(facingDirection, baitSpeed);
        }

        baitReadyTime = Time.time + baitCooldown;
    }

    private void ConfigureCollisionSafety()
    {
        if (rb != null)
        {
            rb.simulated = true;
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        }

        Collider2D rootCollider = GetComponent<Collider2D>();
        if (rootCollider != null)
        {
            rootCollider.isTrigger = false;
            rootCollider.enabled = true;
        }

        int playerLayer = gameObject.layer;
        for (int layer = 0; layer < 32; layer++)
        {
            Physics2D.IgnoreLayerCollision(playerLayer, layer, false);
        }
    }
}
