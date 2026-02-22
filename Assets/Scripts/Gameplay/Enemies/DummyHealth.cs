using UnityEngine;

public class DummyHealth : MonoBehaviour, IDamageable
{
    [SerializeField] private int maxHealth = 3;
    [SerializeField] private bool destroyOnDeath = true;

    private int currentHealth;

    private void Awake()
    {
        currentHealth = Mathf.Max(1, maxHealth);
    }

    public void TakeDamage(int amount)
    {
        int damage = Mathf.Max(1, amount);
        currentHealth -= damage;

        Debug.Log($"{name} took {damage} damage. HP: {currentHealth}");

        if (currentHealth <= 0)
        {
            MarkableEnemy markable = GetComponent<MarkableEnemy>();
            if (markable != null)
            {
                markable.Unmark();
            }

            if (destroyOnDeath)
            {
                Destroy(gameObject);
            }
        }
    }
}
