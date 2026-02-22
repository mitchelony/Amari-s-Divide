using UnityEngine;

[DisallowMultipleComponent]
public class EnemySpawnPoint : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject regularEnemyPrefab;
    public GameObject tricksterEnemyPrefab;

    [Header("Group")]
    public int minGroupSize = 2;
    public int maxGroupSize = 4;
    public float activationDistance = 12f;
    [Range(0f, 1f)] public float tricksterChance = 0.2f;
    public float spawnRadius = 1.5f;
    public int maxTrickstersPerGroup = 1;
    public bool activateOnce = true;
    public bool useObjectPooling = true;

    private Transform player;
    private bool activated;

    private void Update()
    {
        if (activateOnce && activated)
        {
            return;
        }

        if (player == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
            {
                player = playerObject.transform;
            }
            else
            {
                return;
            }
        }

        if (Vector2.Distance(player.position, transform.position) > activationDistance)
        {
            return;
        }

        SpawnGroup();

        if (activateOnce)
        {
            activated = true;
        }
    }

    private void SpawnGroup()
    {
        int minCount = Mathf.Max(1, minGroupSize);
        int maxCount = Mathf.Max(minCount, maxGroupSize);
        int count = Random.Range(minCount, maxCount + 1);

        int trickstersSpawned = 0;
        for (int i = 0; i < count; i++)
        {
            bool canSpawnTrickster = tricksterEnemyPrefab != null && trickstersSpawned < maxTrickstersPerGroup;
            bool useTrickster = canSpawnTrickster && Random.value < tricksterChance;

            GameObject prefab = useTrickster ? tricksterEnemyPrefab : regularEnemyPrefab;
            if (prefab == null)
            {
                prefab = regularEnemyPrefab != null ? regularEnemyPrefab : tricksterEnemyPrefab;
            }

            if (prefab == null)
            {
                continue;
            }

            Vector2 offset = Random.insideUnitCircle * Mathf.Max(0f, spawnRadius);
            Vector3 spawnPosition = new Vector3(transform.position.x + offset.x, transform.position.y + offset.y, transform.position.z);

            GameObject spawned = useObjectPooling
                ? EnemyPool.Spawn(prefab, spawnPosition, Quaternion.identity)
                : InstantiateEnemy(prefab, spawnPosition, Quaternion.identity);

            if (spawned == null)
            {
                Debug.LogWarning($"Failed to spawn enemy from prefab on {name}.");
            }

            if (useTrickster)
            {
                trickstersSpawned++;
            }
        }
    }

    private GameObject InstantiateEnemy(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        Object spawned = Instantiate((Object)prefab, position, rotation);
        if (spawned is GameObject gameObject)
        {
            return gameObject;
        }

        if (spawned is Component component)
        {
            return component.gameObject;
        }

        return null;
    }
}
